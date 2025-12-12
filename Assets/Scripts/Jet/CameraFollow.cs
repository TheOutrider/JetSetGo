// using Cinemachine;
// using UnityEngine;

// public class CameraFollow : MonoBehaviour
// {
//     public CinemachineFollow virtualCamera;

//     public static CameraFollow Singleton
//     {
//         get => _singleton;
//         set
//         {
//             if (value == null)
//                 _singleton = null;
//             else if (_singleton == null)
//                 _singleton = value;
//             else if (_singleton != value)
//             {
//                 Destroy(value);
//                 Debug.LogError($"There should only ever be one instance of {nameof(CameraFollow)}!");
//             }
//         }
//     }
//     private static CameraFollow _singleton;

//     private Transform target;

//     private void Awake()
//     {
//         Singleton = this;
//     }

//     private void OnDestroy()
//     {
//         if (Singleton == this)
//             Singleton = null;
//     }

//     private void LateUpdate()
//     {
//         if (target != null)
//         {
//             transform.SetPositionAndRotation(target.position, target.rotation);
//         }
//     }

//     public void SetTarget(Transform newTarget)
//     {
//         target = newTarget;
//     }

//     public void SetVirtualCamLookAndFollow(Transform newTarget)
//     {
//         if (virtualCamera != null)
//         {
//             virtualCamera.Follow = newTarget;
//             virtualCamera.LookAt = newTarget;
//         }
//         else
//         {
//             Debug.LogWarning("Virtual Camera is not assigned.");
//         }
//     }

//     public void HandleVirtualCamDutch(float dutchValue)
//     {
//         virtualCamera.m_Lens.Dutch = dutchValue;
//     }
// }
