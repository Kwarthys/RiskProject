using UnityEngine;
using System.Collections;

public class SeaRoad{

    private int[] p1 = new int[2];
    private int[] p2 = new int[2];
    private State s1;
    private State s2;

    public SeaRoad(int[] pp1, int[] pp2, State ps1, State ps2)
    {
        p1[0] = pp1[0]; p1[1] = pp1[1];
        p2[0] = pp2[0]; p2[1] = pp2[1];

        s1 = ps1; s2 = ps2;
    }

    public double size()
    {
        return MyMaths.getDistance(p1, p2);
    }

    public int[][] getLimits()
    {
        int[][] tmp = { p1, p2 };
        return tmp;
    }

    public State[] getLinkedStates()
    {
        State[] tmp = { s1, s2 };
        return tmp;
    }

    public bool isInterState()
    {
        return (s1 == s2);
    }

    public void rebase(State fromState, State toState)
    {
        if (s1 == fromState) s1 = toState;
        if (s2 == fromState) s2 = toState;
    }
}
