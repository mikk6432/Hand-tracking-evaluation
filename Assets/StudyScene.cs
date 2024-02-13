using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;

public class StudyScene : MonoBehaviour
{

    private StreamWriter writer;
    private GameObject sphere;
    private GameObject fingerTipMock;
    public float speed = 5f;
    private float originalX;
    private string filePath;

    // Start is called before the first frame update
    void Start()
    {
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(0, 1.5f, 0);
        float sphereSize = 0.04f;
        sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
        fingerTipMock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fingerTipMock.transform.position = new Vector3(0, 1.5f, 0);
        fingerTipMock.transform.localScale = new Vector3(0.5f * sphereSize, 0.5f * sphereSize, 0.5f * sphereSize);

        // Set the file path where the data will be saved
        filePath = "distance_data.csv";

        // Delete previous files
        if (File.Exists(filePath))
        {
            // Delete the file
            File.Delete(filePath);
        }


        writer = new StreamWriter(filePath, true);
        writer.WriteLine("Time,Distance");
    }

    // Update is called once per frame
    void Update()
    {
        float cmToEachSide = 0.1f;
        float newX = originalX + Mathf.Sin(Time.time * speed) * cmToEachSide;

        // Update the sphere's position
        sphere.transform.position = new Vector3(newX, sphere.transform.position.y, sphere.transform.position.z);

        float distance = Vector3.Distance(sphere.transform.position, fingerTipMock.transform.position);

        // writer.WriteLine($"{Time.time:F2},{distance:F4}");
        string timeString = Time.time.ToString(CultureInfo.InvariantCulture);
        string distanceString = distance.ToString(CultureInfo.InvariantCulture);

        // Concatenate the strings with a single comma
        string dataLine = timeString + "," + distanceString;

        // Write the formatted string to the file
        writer.WriteLine(dataLine);
    }

    void OnDisable()
    {
        // Make sure to flush and close the StreamWriter when the object is disabled or destroyed
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }
    }
}
