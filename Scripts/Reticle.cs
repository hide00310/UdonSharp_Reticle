
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine;

public class Reticle : UdonSharpBehaviour
{
    public Transform Head;
    public Transform ShootPos;
    public Transform HitPos;
    public Transform ReticleImage;

    public float RaycastPointTau = 0.5f;
    public float RaycastNormalTau = 0.5f;

    private Ray ReticleRay = new Ray();
    private Vector3 RaycastPoint = Vector3.zero;
    private Vector3 RaycastNormal = Vector3.zero;
    private Vector3 PrevRaycastPoint = Vector3.zero;
    private Vector3 PrevRaycastNormal = Vector3.zero;
    private int UiLayerMask;

    void Start()
    {
        UiLayerMask = LayerMask.GetMask("UI");
        RaycastPoint = ReticleImage.position;
        RaycastNormal = ReticleImage.forward;
        PrevRaycastPoint = ReticleImage.position;
        PrevRaycastNormal = ReticleImage.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Networking.LocalPlayer == null) return;
        if (!Networking.LocalPlayer.IsValid()) return;

        // 発射位置(ShootPos)からレイキャストする
        ReticleRay.origin = ShootPos.position;
        ReticleRay.direction = ShootPos.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(ReticleRay, out hit))
        {
            // 壁との衝突位置(HitPos)からHeadにレイキャストする
            var trackingHead = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Head.SetPositionAndRotation(
                trackingHead.position,
                trackingHead.rotation
            );
            HitPos.position = hit.point;
            HitPos.LookAt(Head);
            ReticleRay.origin = HitPos.position;
            ReticleRay.direction = HitPos.TransformDirection(Vector3.forward);
            if (Physics.Raycast(ReticleRay, out hit, Mathf.Infinity, UiLayerMask))
            {
                RaycastPoint = hit.point;
                RaycastNormal = hit.normal;
            }
        }
        // ローパスフィルタをかける
        ReticleImage.Translate((RaycastPoint - PrevRaycastPoint) * RaycastPointTau, Space.World);
        ReticleImage.forward = Vector3.Lerp(PrevRaycastNormal, RaycastNormal, RaycastNormalTau);
        for (int i = 0; i < 3; i++)
        {
            PrevRaycastPoint[i] = ReticleImage.position[i];
            PrevRaycastNormal[i] = ReticleImage.forward[i];
        }
    }
}
