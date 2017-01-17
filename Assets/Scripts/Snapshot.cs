using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityStandardAssets.ImageEffects;

public class Snapshot : MonoBehaviour {

    // Methods
    // Path for Snapshot
    public string path = "Assets/Snapshot/";

    // Snapshot Name
    public string snapName = "Snap";

    // The list of snapshots
    public List<string> snapshots = new List<string>();

    // Camera effects

    Camera camera;

    private float minFOV = 30;
    private float maxFOV = 60;
    private float modifFOV = 1;

    // Fake Focus
    private DepthOfField deptOfField;

    private bool currentlyZoomIn = false;
    private bool currentlyZoomOut = false;

    // Use this for initialization
    void Start ()
    {
        camera = GetComponent<Camera>();
        deptOfField = GetComponent<DepthOfField>();
    }

    // Update is called once per frame
    void Update ()
    {
	    if(Input.GetMouseButton(0))
        {
            ZoomIn();

            if (currentlyZoomIn == false)
            {
                currentlyZoomIn = true;
                StopAllCoroutines();
                StartCoroutine(Focus(false));
            }
                
        }
        else
        {
            if (currentlyZoomIn == true)
            {
                currentlyZoomIn = false;
                StopAllCoroutines();
                StartCoroutine(Focus(true));
            }
        }

        if (Input.GetMouseButton(1))
        {
            ZoomOut();
            currentlyZoomOut = true;
            StopAllCoroutines();
            StartCoroutine(Focus(false));
        }
        else
        {
            if (currentlyZoomIn == true)
            {
                currentlyZoomIn = false;
                StopAllCoroutines();
                StartCoroutine(Focus(true));
            }
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

    // Camera effects
    void ZoomIn()
    {
        if(camera.fieldOfView> minFOV)
        {
            camera.fieldOfView -= modifFOV;
        }

    }

    void ZoomOut()
    {
        if (camera.fieldOfView < maxFOV)
        {
            camera.fieldOfView += modifFOV;
        }
    }

    IEnumerator Focus(bool _focus)
    {
        if(_focus)
        {
            while (deptOfField.aperture < 1)
            {
                deptOfField.aperture += 0.1f;

                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            while (deptOfField.aperture > 0)
            {
                deptOfField.aperture -= 0.1f;

                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForEndOfFrame();
    }

}
