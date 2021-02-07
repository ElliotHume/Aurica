AdVd Glyph Recognition tool

This package contains:

- Scripts
 · Stroke class, Glyph ScriptableObject and GlyphSet ScriptableObject.
 · GlyphDrawInput component that reads pointer input and handles Matching Methods.
 · 5 MatchingMethods (SqrDistanceDTW, SqrDTWMatch MemoryCost, SqrDistance, SqrMemory and Legendre/Legendre-Sobolev orthogonal series method).
 · Legendre/Legendre-Sobolev series generators that use Stroke as input.
 · GlyphMatch class that stores match info. Includes: source glyph and best target, cost of the match (difference between the glyphs), stroke matchings and point-to-point matchings.
 · GlyphDisplay class that draws a Glyph using a StrokeGraphic component.
 · 2 StrokeGraphic components that draw strokes in two different ways (Cap&Stretch and RepeatTexture).
 · GlyphDrawer component that handles the drawing of strokes for the GlyphDrawInput component.
 · Glyph Editor and EditorWindow (graphical editor), and other editors.

- SampleScene

- SampleAssets
 · GlyphInput prefabs ready to be added to the canvas and be customized.
 · Materials and Textures for stroke rendering.
 · Extra GlyphDrawer implementation.
 · Sample listener script to read the result.

- SampleGlyphs
 · Collection of 20 Glyphs



Instructions:

- Create your own glyphs. Assets -> Create -> Glyph and Show Editor (or Window -> Glyph Editor and then select the new Glyph).

- Glyph Editor. Add new strokes in the editor and shape them using the edition tools. Make sure your glyphs are normalized and have a common sample distance of your choice.

- Add a Glyph Input (fast). Move the Glyph Input from SampleAssets to your canvas. Set the parameters and assign your own GlyphSet with your target glyphs.

- Add a Glyph Input (custom). GameObject -> UI -> AdVd -> Glyph Input. Set your prefered matching method and parameters, and assign a GlyphSet that contains your target glyphs. Now you are ready cast glyphs on your input but the strokes and glyphs won't be drawn.
- Drawing stroke input. Add a GlyphDrawer component to your GlyphInput, assign your own StrokeGraphic components in child objects (Add Component -> Scripts -> AdVd.GlyphRecognition -> *StrokeGraphic) or create your own GlyphDrawer to customize how is everything drawn.

- Get results. Add a listener to the OnGlyphCast event to receive the info of the best match. The event contains the index of the closest target glyph and a GlyphMatch object.
 · The listener function should look like this: public void GlyphCastResult(int index, AdVd.GlyphRecognition.GlyphMatch match)



Matching Methods:
- SqrDistanceDTW: DTW using square distance. This works quite well but a small offset on a long line may cause huge errors. Stroke time cost: O(n^2).

- SqrDTWMatch MemoryCost: This method will do the same match as SqrDistanceDTW, but the cost will be obtained using a formula that weights the direction more than the position of the stroke.

- SqrDistance: This method is worse than DTW and may fail when facing sharp angles or loops. However this is usually faster than DTW. O(n) vs O(n^2) time.

- SqrMemory: This method will use the same algorithm of SqrDistance with the formula of SqrDTWMatch MemoryCost, giving slightly better results but also in time O(n).

- Legendre/Legendre-Sobolev orthogonal series: Precomputes coefficients for the target glyphs in time O(n) and gets the cost in constant time afterwards. Point to point matching is still O(n) however.
 · Links to papers about these methods:
	http://www.csd.uwo.ca/~watt/pub/reprints/2009-drr-legendre-sobolev.pdf
	http://www.cvc.uab.es/icdar2009/papers/3725b265.pdf



Video Tutorial:
	https://www.youtube.com/watch?v=ketcmvi3EN4