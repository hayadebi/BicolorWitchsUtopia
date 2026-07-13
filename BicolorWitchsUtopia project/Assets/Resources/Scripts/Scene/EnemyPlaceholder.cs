using UnityEngine;
using BicolorWitch.Game;
using BicolorWitch.Effect;

namespace BicolorWitch.Enemy
{
    public class EnemyPlaceholder : MonoBehaviour
    {
        void Awake()
        {
            // GameStateDependentObjectをアタッチ
            if (GetComponent<GameStateDependentObject>() == null)
            {
                gameObject.AddComponent<GameStateDependentObject>();
            }
            // OffCameraCullingをアタッチ
            if (GetComponent<OffCameraCulling>() == null)
            {
                gameObject.AddComponent<OffCameraCulling>();
            }
        }

        void Start()
        {
            Debug.Log($"EnemyPlaceholder {gameObject.name} が初期化されました。", this);
        }
    }
}
