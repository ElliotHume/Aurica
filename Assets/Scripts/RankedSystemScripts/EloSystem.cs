using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EloSystem {

    public static float KFactor = 32f;
    public static float InitialElo = 1500f;

    // Function to calculate the Probability
    static float Probability(float rating1, float rating2) {
        return 1.0f * 1.0f / (1f + 1.0f * (float)(Mathf.Pow(10f, 1.0f * (rating1 - rating2)/ 400f)));
    }

    // Function to calculate Elo rating
    // d determines whether Player A wins or loses
    public static float EloRating(float localPlayerElo, float opponentElo, bool d) {
        float Ra = localPlayerElo;
        float Rb = opponentElo;

        // To calculate the Winning
        // Probability of Player B
        float Pb = Probability(Ra, Rb);
 
        // To calculate the Winning
        // Probability of Player A
        float Pa = Probability(Rb, Ra);
 
        // Case -1 When Player A wins
        // Updating the Elo Ratings
        if (d == true) {
            Ra = Ra + KFactor * (1f - Pa);
            Rb = Rb + KFactor * (0f - Pb);
        }
 
        // Case -2 When Player B wins
        // Updating the Elo Ratings
        else {
            Ra = Ra + KFactor * (0f - Pa);
            Rb = Rb + KFactor * (1f - Pb);
        }
 
        Debug.Log("Updated Ratings:-\n");
 
        Debug.Log(
            "LocalPlayer = "
            + (Mathf.Round(Ra * 1000000.0f) / 1000000.0f)
            + " Opponent = "
            + Mathf.Round(Rb * 1000000.0f) / 1000000.0f);

        return Ra;
    }
}
