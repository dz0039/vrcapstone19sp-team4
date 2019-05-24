namespace HomeRun.Net
{
	using UnityEngine;
	using System.Collections;
	using HomeRun.Game;

	// This component handles network coordination for moving balls.
	// Synchronizing moving objects that are under the influence of physics
	// and other forces is somewhat of an art and this example only scratches
	// the surface.  Ultimately how you synchronize will depend on the requirements
	// of your application and its tolerance for users seeing slightly different
	// versions of the simulation.
	public class P2PNetworkBall : MonoBehaviour
	{
		int m_id = -1;
		private BallType m_ballType = BallType.FastBall;

		// cached reference to the GameObject's Rigidbody component
		private Rigidbody m_rigidBody;
		private ThrownBall m_tb;

		void Awake()
		{
			m_rigidBody = gameObject.GetComponent<Rigidbody>();
			m_tb = gameObject.GetComponent<ThrownBall>();
		}

		public ThrownBall ThrowBall {
			get { return m_tb; }
			set { m_tb = value; }
		}

		public BallType BallType {
			get { return m_ballType; }
		}

		public int InstanceID {
			get { return m_id; }
		}

		public P2PNetworkBall SetType(BallType t) {
			m_ballType = t;
			return this;
		}

		public P2PNetworkBall SetInstanceID(int id) {
			m_id = id;
			return this;
		}

		public void ProcessBallThrow(Vector3 pos, Vector3 vel) {
			m_tb.transform.position = pos;
			m_tb.GrabEnd(vel, Vector3.zero);
			m_rigidBody.isKinematic = false;
		}

		public void ProcessBallHit(Vector3 pos, Vector3 vel) {
			Debug.Log("Hit!" + vel);
            m_rigidBody.useGravity = true;
			//m_rigidBody.isKinematic = true;
			m_rigidBody.angularVelocity = Vector3.zero;
			m_rigidBody.velocity = Vector3.zero;
			m_tb.transform.position = pos;
			m_rigidBody.velocity = vel;
			NetEffectController.Instance.PlayBatHitEffect(pos);
		}

		void OnDestroy()
		{
			// called on ThrownBall
			PlatformManager.P2P.RemoveNetworkBall(gameObject);
		}

	}
}
