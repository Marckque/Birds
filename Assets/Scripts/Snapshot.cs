using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Snapshot : MonoBehaviour {

    // Methods
    // Path for Snapshot
    public string path = "Assets/Snapshot/";

    // Snapshot Name
    public string snapName = "Snap";

    // The list of snapshots
    public List<string> snapshots = new List<string>();

	// Use this for initialization
	void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
	    if(Input.GetMouseButtonDown(0))
        {
            TakeAScreenShot();
        }
	}

    void TakeAScreenShot()
    {
        string _snapName = path + snapName + SnapNumber() + ".png";

        Application.CaptureScreenshot(_snapName);

        snapshots.Add(_snapName);
    }

    // Formalize the Snap number
    string SnapNumber()
    {
        if(snapshots.Count<99)
        {
            if (snapshots.Count < 9)
            {
                return (("00" + (snapshots.Count+1).ToString()));
            }
            else
            {
                return (("0" + (snapshots.Count + 1).ToString()));
            }
        }
        else
        {
            return ((snapshots.Count + 1).ToString());
        }
    }

    void ResetScreenShots()
    {
        foreach(string _path in snapshots)
        {
            File.Delete(_path);
        }

        snapshots = new List<string>();
    }

}
