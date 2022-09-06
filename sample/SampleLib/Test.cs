using System;
using System.Collections.Generic;

namespace Test;

public static class ParameterCheck
{

    public static void M1(this int n) 
    {
    }

    public static void M2(params int[] values)
    {
    }

    public static void M3(in int param = 1)
    {
    }

    public static void M4(out int param)
    {
        param = 1;
    }
    
    public static void M5(ref int param)
    {
        param = 1;
    }

    public static void M6(int v = 1)
    {
    }
    
    public static void M6(int? v)
    {
    }
}