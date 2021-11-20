using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateChooser : MonoBehaviour
{
   
    public GameObject[] roomTemplates;
    // Start is called before the first frame update
    void Start()
    {
        //selects a random room type to spawn
        int rand = Random.Range(0, roomTemplates.Length);
        GameObject instance = Instantiate(roomTemplates[rand], transform.position, Quaternion.identity);
        instance.transform.parent = transform;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
