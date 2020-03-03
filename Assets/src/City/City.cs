using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City {
    private static City instance;

    private City()
    {

    }

    public static City Instance
    {
        get {
            if(instance == null) {
                instance = new City();
            }
            return instance;
        }
    }
}
