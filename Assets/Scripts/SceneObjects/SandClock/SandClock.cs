using System.Collections;
using System.Net.Sockets;
using QFramework;
using Unity.VisualScripting;
using UnityEngine;
namespace SceneObject
{
    public class SandClock : MonoBehaviour
    {
        public GameObject HalfA;
        public GameObject HalfB;
        Quaternion rot;
        int currentrot;
        public static Vector3 centerPoint;
        private void Awake()
        {
            rot = transform.rotation;
            centerPoint = transform.position;
            TypeEventSystem.Global.Register<DayLightController.TwenSecCountEvent>(e => Flip()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        public void Flip()
        {
            StartCoroutine(IEFlip());
        }
        IEnumerator IEFlip()
        {
            HalfA.SetActive(false);
            while(!(currentrot==180))
            {
                transform.Rotate(new Vector3(0, 0, 5));
                currentrot += 5;
                yield return new WaitForFixedUpdate();
            }
            HalfA.SetActive(true);
            Initialize();
            HalfA.GetComponent<HalfA>().Flow();
            HalfB.GetComponent<HalfB>().Flow();
            currentrot = 0;
        }
        public void Initialize()
        {
            transform.SetPositionAndRotation(transform.position, rot);
            HalfB.GetComponent<HalfB>().Initialize();
            HalfA.GetComponent<HalfA>().Initialize();
        }
    }
}