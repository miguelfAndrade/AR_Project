using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Kudan.AR.Samples;

public class DetectLocation : MonoBehaviour 
{

	private Vector2[] targetCoordinates;
	private Vector2 deviceCoordinates;
	private float distanceFromTarget = 0.002f;
	private float proximity = 0.001f;
	private float sLatitude, sLongitude;
	private bool enableByRequest = true;
	public int maxWait = 10;
	public bool ready = false;
	public Text text;
	public Text text2;
	public Text text3;
	public GameObject[] obj;
	public SampleApp sa;

	LocationService service;
	private float dt;
	private float ang;

	private int num = 0;
	private int[] entrou2 = { 0, 0, 0, 0, 0, 0 };
	private float prox;
	private int[] entrou = {0,0,0,0,0,0};
	private Vector2 user;

	private float nextActionTime = 0.0f;
	private float period = 0.5f;

	//experimentação
	//public Camera cam;

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < obj.Length; i++) 
		{
			obj [i].SetActive (false);
		}
		targetCoordinates = new Vector2[] 
		{
			//new Vector2 (41.695065f, -8.831291f),
			//new Vector2 (41.693592f, -8.828246f),
			//new Vector2 (41.693293f, -8.827334f),
			//new Vector2 (41.690601f, -8.827663f),
			//new Vector2 (41.690454f, -8.829037f),
			//new Vector2 (41.690059f, -8.830244f)
			new Vector2 (41.694266f, -8.846686f),
			new Vector2 (41.694260f, -8.845870f),
			new Vector2 (41.694260f, -8.845870f),
			new Vector2 (41.695105f, -8.847804f),
			new Vector2 (41.696196f, -8.847120f),
			new Vector2 (41.694202f, -8.847569f),
		};
		StartCoroutine (getLocation ());
	}

	IEnumerator getLocation(){
		service = Input.location;
		if (!enableByRequest && !service.isEnabledByUser) {
			Debug.Log("Location Services not enabled by user");
			yield break;
		}
		service.Start();
		while (service.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		if (maxWait < 1){
			Debug.Log("Timed out");
			yield break;
		}
		if (service.status == LocationServiceStatus.Failed) {
			Debug.Log("Unable to determine device location");
			yield break;
		} else {
			//text.text = "Target Location : "+dLatitude + ", "+dLongitude+"\nMy Location: " + service.lastData.latitude + ", " + service.lastData.longitude;
			sLatitude = service.lastData.latitude;
			sLongitude = service.lastData.longitude;
		}
		//service.Stop();
		ready = true;
		startCalculate ();
	}


	// Update is called once per frame
	void Update () 
	{
		if (Time.time > nextActionTime ) 
		{
			nextActionTime += period;
			posActualiza ();
		}
	}

	public void posActualiza ()
	{
		sLatitude = service.lastData.latitude;
		sLongitude = service.lastData.longitude;
		user.x = sLatitude;
		user.y = sLongitude;
		text2.text = "My Location: " + sLatitude + ", " + sLongitude + ", " + dt;
		prox = Vector2.Distance (targetCoordinates [num], user);
		text.text = "Distance : " + prox.ToString ();
		if (sLatitude == targetCoordinates [num].x && sLongitude == targetCoordinates [num].y && entrou[num] == 0) 
		{
			entrou [num] = 1;
			num++;
		}
		startCalculate ();
	}

	public void startCalculate()
	{
		deviceCoordinates = new Vector2 (sLatitude, sLongitude);
		for (int i = 0; i < obj.Length; i++) 
		{
			proximity = Vector2.Distance (targetCoordinates[i], deviceCoordinates);
			if (proximity <= distanceFromTarget) 
			{
				//text3.text = entrou2 [i].ToString ();
				obj[i].SetActive (true);
				if (entrou2 [i] == 0) 
				{
					
					distance (deviceCoordinates.x, deviceCoordinates.y, targetCoordinates [i].x, targetCoordinates [i].y);
					posObj (obj[i]);
					entrou2 [i] = 1;
					sa.StartClicked ();
				}
				//sa.StartClicked ();
			} else if(proximity > distanceFromTarget)
			{
				obj [i].SetActive (false);
				entrou2 [i] = 0;
			}
		}
	}

	public void distance(float lat1, float lon1, float lat2, float lon2)
	{
		//--Calcula a distância entre os pontos em Quilometros
		int R = 6371; //raio da Terra
		float lati1 = lat1 * Mathf.Deg2Rad;
		float lati2 = lat2 * Mathf.Deg2Rad;
		float dLat = (lat2-lat1) * Mathf.Deg2Rad;
		float dLon = (lon2-lon1) * Mathf.Deg2Rad;

		float a = Mathf.Sin(dLat/2) * Mathf.Sin(dLat/2) + (Mathf.Cos(lati1) * Mathf.Cos(lati2)) * (Mathf.Sin(dLon/2) * Mathf.Sin(dLon/2));
		float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1-a));

		//--Calcula o angulo entre os dois pontos
		float y = Mathf.Sin(dLon) * Mathf.Cos(lati2);
		float x = Mathf.Cos(lati1)*Mathf.Sin(lati2) - Mathf.Sin(lati1)*Mathf.Cos(lati2)*Mathf.Cos(dLon);

		ang = Mathf.Atan2(y, x);//angulo final

		dt = R * c;//distância final
	}

	public void posObj(GameObject obj)
	{
		float x; //Posição Virtual em x
		float y; //Posição Virtual em z
		float dtV; //Distância Virtual

		dtV = dt*1000 * 600;

		x = dtV * Mathf.Cos (ang);
		y = dtV * Mathf.Sin (ang);

		obj.transform.Translate (x, 0, y);
	}
}

