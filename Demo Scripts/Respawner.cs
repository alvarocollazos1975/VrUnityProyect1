using UnityEngine;

namespace BIMOS.Samples
{
    [AddComponentMenu("BIMOS/Respawner")]
    public class Respawner : MonoBehaviour
    {
        [SerializeField]
        private bool _respawnOnAwake;

        [SerializeField]
        private GameObject _prefab;

        private GameObject _instance;

        private void Awake()
        {
            if (_respawnOnAwake)
                Respawn();
        }

        public void Respawn()
        {
            Destroy(_instance);
            _instance = Instantiate(_prefab, transform.position, transform.rotation);
        }
    }
}
