using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;


namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// UI component to draw glyphs and find the closest match within a set of stored glyphs using a specific matching method.
	/// </summary>
	[RequireComponent(typeof(RectTransform), typeof(Image))]
	public class GlyphDrawInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {

		List<Vector2> stroke;
		List<Stroke> strokeList;
		public Glyph castedGlyph;
		public GlyphMatch currentMatch;
		/// <summary>
		/// The set of glyphs to compare with the casted glyph.
		/// </summary>
		public GlyphSet targetGlyphSet;

		/// <summary>
		/// The size of a normalized glyph relative to the component.
		/// </summary>
		public float normalizedGlyphSize=0.8f;
		/// <summary>
		/// The sample distance when drawing and resampling glyphs.
		/// </summary>
		public float sampleDistance=0.05f;
		/// <summary>
		/// Set castOnTap to true if you want to trigger a glyph cast by tapping on the component.
		/// </summary>
		public bool castOnTap=true;
		/// <summary>
		/// Set to true to override the default theshold used by the matching method.
		/// </summary>
		public bool overrideThreshold=false;//This should be set to true before any change to Threshold.
		//If a change is made to the threshold and then overrideThreshold is set to false, the change will remain and disappear when the method is reinstanced
		[HideInInspector][SerializeField]
		private float threshold=0.09f;
		/// <summary>
		/// Gets or sets the threshold used by the matching method. The field overrideThreshold must be true in order to set the threshold.
		/// </summary>
		/// <value>The threshold.</value>
		public float Threshold{
			get{ 
				if (overrideThreshold){
					return threshold;
				}
				else{
					if (method==null){
						switch(matchingMethod){
						case Matching_Method.SqrDistanceDTWMatchingMethod: return SqrDistanceDTWMatchingMethod.defaultThreshold;
						case Matching_Method.SqrDTWMatchingMemoryCostMethod: return SqrDTWMatchingMemoryCostMethod.defaultThreshold;
						case Matching_Method.SqrDistanceMatchingMethod: return SqrDistanceMatchingMethod.defaultThreshold;
						case Matching_Method.SqrMemoryMatchingMethod: return SqrMemoryMatchingMethod.defaultThreshold;
						case Matching_Method.LegendreMatchingMethod: return LegendreMatchingMethod.defaultThreshold;
						default: return threshold;
						}
					} 
					else{
						return method.threshold; 
					}
				}
			}
			set{ 
				if (overrideThreshold){
					this.threshold=(value>0f?value:0f);//<=0 is ignored
					if (method!=null) method.threshold=this.threshold; 
				}
			}
		}

		public enum Matching_Method{ None=-1, SqrDistanceDTWMatchingMethod, SqrDTWMatchingMemoryCostMethod,
			SqrDistanceMatchingMethod, SqrMemoryMatchingMethod, LegendreMatchingMethod };
		[HideInInspector][SerializeField]
		Matching_Method matchingMethod=Matching_Method.None;
		/// <summary>
		/// Gets or sets the matching method. Re-setting the method (Method=Method) re-instances it.
		/// </summary>
		/// <value>The method.</value>
		public Matching_Method Method{//Method=Method : Re-instaces method
			get{
				return matchingMethod;
			}
			set{
				if (value == Matching_Method.None) method=null;
				else if (value == Matching_Method.SqrDistanceDTWMatchingMethod) method=new SqrDistanceDTWMatchingMethod();
				else if (value == Matching_Method.SqrDTWMatchingMemoryCostMethod) method=new SqrDTWMatchingMemoryCostMethod(alpha);
				else if (value == Matching_Method.SqrDistanceMatchingMethod) method=new SqrDistanceMatchingMethod();
				else if (value == Matching_Method.SqrMemoryMatchingMethod) method=new SqrMemoryMatchingMethod(alpha);
				else if (value == Matching_Method.LegendreMatchingMethod){
					if (generator==Series_Generator.LegendreSeries) method=new LegendreMatchingMethod(new LegendreSeries(12));
					else if (generator==Series_Generator.LegendreSobolevSeries) method=new LegendreMatchingMethod(new LegendreSobolevSeries(12, sf));
					else method=null;
				}
				else method=null;
				if (method!=null && overrideThreshold) method.threshold=this.threshold; 
				matchingMethod=value;
			}
		}
		MatchingMethod method;

		public enum Series_Generator{ None=-1, LegendreSeries, LegendreSobolevSeries }
		[HideInInspector][SerializeField]
		Series_Generator generator=Series_Generator.None;
		/// <summary>
		/// Gets or sets the series generator.
		/// </summary>
		/// <value>The series generator.</value>
		public Series_Generator SeriesGenerator{
			get{
				return generator;
			}
			set{
				generator=value;
				if (matchingMethod==Matching_Method.LegendreMatchingMethod){
					Method=matchingMethod;//method=null;
				}
			}
		}
		
		[HideInInspector][SerializeField]
		float alpha=0.5f;
		/// <summary>
		/// Gets or sets the alpha value used in "memory" matching methods. The bigger is alpha, more error is forgiven.
		/// </summary>
		/// <value>The alpha.</value>
		public float Alpha{
			get{ return alpha; }
			set{ 
				alpha=Mathf.Clamp01(value);
				if (matchingMethod==Matching_Method.SqrDTWMatchingMemoryCostMethod || matchingMethod==Matching_Method.SqrMemoryMatchingMethod){
					method=null;//Method=matchingMethod;
				}
			}
		}
		[HideInInspector][SerializeField]
		float sf=1f;
		/// <summary>
		/// Gets or sets the factor used in Legendre-Sobolev series generator. For a value of 0 Legendre-Sobolev and Legendre series are the same.
		/// </summary>
		/// <value>The Sovolev Factor.</value>
		public float SobolevFactor{
			get{ return sf; }
			set{ 
				sf=(value<0?0:value);
				if (matchingMethod==Matching_Method.LegendreMatchingMethod && generator==Series_Generator.LegendreSobolevSeries){
					method=null;//Method=matchingMethod;
				}
			}
		}

		/// <summary>
		/// Glyph cast event. It contains the index of the closest glyph matched and the info of the match.
		/// </summary>
		[Serializable]
		public class GlyphCastEvent : UnityEvent<int,GlyphMatch>{};
		/// <summary>
		/// The event to listen for glyph casts.
		/// </summary>
		public GlyphCastEvent OnGlyphCast;


//		public delegate void MatchResult(int selectedIndex, GlyphMatch match, float score);
//		public MatchResult OnGlyphCast;
		public delegate void StrokeDraw(Stroke[] strokes);
		/// <summary>
		/// Delegate called when a new stroke is finished.
		/// </summary>
		public StrokeDraw OnStrokeDraw;
		public delegate void PointDraw(Vector2[] points);
		/// <summary>
		/// Delegate called when the stroke currently being drawn changes.
		/// </summary>
		public PointDraw OnPointDraw;

		void Start () {
			Method=Method;//Instances method
		}

		Vector2 prevPos;
		bool RectEventPoint(Vector2 position, Camera pressEventCamera, out Vector2 localPoint){
			RectTransform rt =  transform as RectTransform;
			Rect r = rt.rect;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, pressEventCamera, out localPoint);

			localPoint-=r.center;
			localPoint.x/=r.width*normalizedGlyphSize; localPoint.y/=r.height*normalizedGlyphSize;
			return RectTransformUtility.RectangleContainsScreenPoint(rt, position, pressEventCamera);
		}

		#region IBeginDragHandler implementation

		public void OnBeginDrag (PointerEventData eventData)
		{
			if (eventData.button!=PointerEventData.InputButton.Left) return;
			stroke=new List<Vector2>();
			Vector2 localPoint;
			if (RectEventPoint(eventData.pressPosition, eventData.pressEventCamera, out localPoint)) stroke.Add (prevPos=localPoint);
		}

		#endregion

		#region IDragHandler implementation

		public void OnDrag (PointerEventData eventData)
		{
			if (eventData.button!=PointerEventData.InputButton.Left) return;
			if (stroke!=null){
				Vector2 currPos;
				if (RectEventPoint(eventData.position, eventData.pressEventCamera, out currPos)){
					if (sampleDistance<Stroke.minSampleDistance){//No resample
						stroke.Add(currPos);
					}
					else{//Resample
						Vector2 dir=(currPos-prevPos);
						float dist=dir.magnitude;
						if (dist>0) dir/=dist;
						while(dist>sampleDistance){
							Vector2 point=prevPos+dir*sampleDistance;
							stroke.Add (point);
							prevPos=point;
							dist-=sampleDistance;
						}
					}
					if (OnPointDraw!=null){
						Vector2[] points=new Vector2[stroke.Count+1];
						stroke.CopyTo(points); points[points.Length-1]=currPos;
						OnPointDraw(points);
					}
				}
			}
		}

		#endregion

		#region IEndDragHandler implementation

		public void OnEndDrag (PointerEventData eventData)
		{
			if (eventData.button!=PointerEventData.InputButton.Left) return;
			if (stroke!=null){
				if (stroke.Count<2){
					stroke=null; 
					if (OnPointDraw!=null) OnPointDraw(null);
					return;
				}
				Vector2 currPos;
				if (RectEventPoint(eventData.position, eventData.pressEventCamera, out currPos)) stroke.Add(currPos);
				if (strokeList==null) strokeList=new List<Stroke>();
				Stroke newStroke=new Stroke(stroke.ToArray());
				strokeList.Add(newStroke);
				stroke=null;
				if (OnStrokeDraw!=null) OnStrokeDraw(strokeList.ToArray());
			}
		}

		#endregion

		#region IPointerClickHandler implementation

		public void OnPointerClick (PointerEventData eventData)
		{
			if (eventData.button!=PointerEventData.InputButton.Left) return;
			if (stroke==null && castOnTap){
				Cast();
			}
		}
		#endregion
		
		/// <summary>
		/// Casts the currently drawn glyph. Return false if there is no glyph to cast. 
		/// Use PerformCast(true) to recast.
		/// </summary>
		/// 
		public bool Cast(){
			if (strokeList!=null){
				if (strokeList.Count>0){
					Glyph newGlyph=Glyph.CreateGlyph(strokeList.ToArray(), sampleDistance);
					newGlyph.name="NewGlyph ["+this.name+"]";
					Cast(newGlyph);
					strokeList.Clear();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Cast the specified glyph. Return true if glyph is not null.
		/// </summary>
		/// <param name="glyph">Glyph.</param>
		public bool Cast(Glyph glyph){
			castedGlyph=glyph;
			if (castedGlyph==null) return false;

			if (method==null) Method=Method;
			if (method!=null && targetGlyphSet!=null){
				GlyphMatch match;
				int index = method.MultiMatch(castedGlyph, targetGlyphSet, out match);
				currentMatch = (index!=-1 ? match : null);
				if (OnGlyphCast!=null) OnGlyphCast.Invoke(index, match);
			}
			return true;
		}

		/// <summary>
		/// Cast the currently drawn glyph or recast previous glyph if there is no glyph drawn.
		/// </summary>
		/// <param name="recast">If set to <c>true</c> and there is no glyph drawn recast previous glyph.</param>
		public void PerformCast(bool recast=false){
			if (!Cast() && recast){
				if (castedGlyph!=null) Cast (castedGlyph);
			}
		}

		/// <summary>
		/// Clears the current input.
		/// </summary>
		public void ClearInput(){
			stroke=null;
			if (strokeList!=null) strokeList.Clear();
		}

		void OnDrawGizmosSelected(){
			Gizmos.matrix=transform.localToWorldMatrix;
			Gizmos.color=Color.red;
			Rect r = (transform as RectTransform).rect;
			Vector2 scale = r.size * normalizedGlyphSize;
			Vector2 position = r.center;
			if (castedGlyph!=null) castedGlyph.DrawGlyph(position, scale);

			Gizmos.color=Color.yellow;
			if (strokeList!=null) foreach(Stroke s in strokeList) s.DrawStroke(position, scale);
			if (stroke!=null && stroke.Count>1){
				Vector2 prevPoint, currPoint=position+Vector2.Scale(stroke[0],scale);
				for (int i = 1; i < stroke.Count; i++){
					prevPoint=currPoint; currPoint=position+Vector2.Scale(stroke[i],scale);
					Gizmos.DrawLine(prevPoint, currPoint);
				}
			}

			if (currentMatch!=null){
				Gizmos.color=Color.blue;
				currentMatch.target.DrawGlyph(position, scale);
			}

			Gizmos.color=new Color(0.8f,0.8f,0.8f);
			Gizmos.DrawWireCube(position,scale);
		}
	}
}
