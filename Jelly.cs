using UnityEngine;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    #region Class

    public class JellyVertex
    {
        public int id;
        public Vector3 position;
        public Vector3 velocity, force;

        public JellyVertex(int id, Vector3 position)
        {
            this.id = id;
            this.position = position;
        }

        public void Shake(Vector3 target, float mass, float stiffness, float density)
        {
            force = (target - position) * stiffness;
            velocity = (velocity + force / mass) * density;
            position += velocity;

            if ((velocity + force + force / mass).magnitude < 0.001f)
                position = target;
        }
    }

    #endregion Class

    public class Jelly : MonoBehaviour
    {
        #region Variables

        private Mesh _originalMesh, _meshClone;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private Vector3[] _vertexArray;
        private JellyVertex[] _jellyVertexArray;

        [SerializeField] private float _mass = 1.25f;
        [SerializeField] private float _damping = 0.75f;
        [SerializeField] private float _intensity = 1f;
        [SerializeField] private float _stiffness = 0.5f;

        #endregion Variables

        #region Unity Functions

        public void Awake()
        {
            _originalMesh = GetComponent<MeshFilter>().sharedMesh;
            _meshClone = Instantiate(_originalMesh);

            _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.sharedMesh = _meshClone;

            _meshRenderer = GetComponent<MeshRenderer>();

            _jellyVertexArray = new JellyVertex[_meshClone.vertices.Length];
            for (int i = 0; i < _meshClone.vertices.Length; i++)
            {
                _jellyVertexArray[i] = new JellyVertex(i, transform.TransformPoint(_meshClone.vertices[i]));
            }
        }

        public void FixedUpdate()
        {
            _vertexArray = _originalMesh.vertices;
            for (int i = 0; i < _jellyVertexArray.Length; i++)
            {
                Vector3 target = transform.TransformPoint(_vertexArray[_jellyVertexArray[i].id]);
                float intensity = (1 - (_meshRenderer.bounds.max.y - target.y) / _meshRenderer.bounds.size.y) * _intensity;
                _jellyVertexArray[i].Shake(target, _mass, _stiffness, _damping);
                target = transform.InverseTransformPoint(_jellyVertexArray[i].position);
                _vertexArray[_jellyVertexArray[i].id] = Vector3.Lerp(_vertexArray[_jellyVertexArray[i].id], target, intensity);
            }

            _meshClone.vertices = _vertexArray;
        }

        #endregion Unity Functions

        #region Functions

        public void Enable()
        {
            enabled = true;
        }

        public void Disable()
        {
            enabled = false;
        }

        public async void Disable(int durationInMilliseconds)
        {
            await Task.Delay(durationInMilliseconds);

            enabled = false;
        }

        #endregion Functions
    }
}