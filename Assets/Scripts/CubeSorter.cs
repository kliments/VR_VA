using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSorter : IComparer {

    // Calls CaseInsensitiveComparer.Compare on the monster name string.
    int IComparer.Compare(System.Object x, System.Object y)
    {
        return ((new CaseInsensitiveComparer()).Compare(((GameObject)x).name, ((GameObject)y).name));
    }
    
}
