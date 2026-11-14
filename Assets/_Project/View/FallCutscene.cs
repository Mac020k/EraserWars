using System.Collections;
using UnityEngine;

namespace Keshipin.View
{
    public sealed class FallCutscene : MonoBehaviour
    {
        [SerializeField] float duration = 0.8f;

        public IEnumerator Play(Transform eraser)
        {
            var t = 0f;
            var start = eraser.position;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                eraser.position = start + Vector3.down * (t / duration) * 0.4f;
                yield return null;
            }
        }
    }
}
