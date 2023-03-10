using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Roots.Characters.Brootus
{
	/// <summary>Governs the mechanics of rooting to "substrate" objects in the world.</summary>
	/// <remarks>
	/// Attach this script to the player character prefab.
	///
	/// \todo Use tag "Substrate" to distinguish objects to which roots may attach from objects to which they cannot.
	/// </remarks>
	public class RootSystem : MonoBehaviour
	{
		[SerializeField] private AudioClip latch;
		[SerializeField] private AudioClip unlatch;
		private AudioSource _source;

		private const float RootDamper = 1000f;
		private const float RootSpring = 200000f;
		private const float RootTolerance = 0.01f;

		[Tooltip("For limiting rotation of the seed while rooted."), SerializeField]
		private float rootedAngularDrag = 20f;

		[Tooltip("The roots provide stability against external forces like wind."), SerializeField]
		private float rootedMass = 1000f;

		[Tooltip("Provide a prefab for the visual representation of the roots."), SerializeField]
		private GameObject rootTendrils;

		private Rigidbody _ground;
		private CapsuleCollider _root;
		private SpringJoint _rootLinkage;
		private GameObject _rootTendrils;
		private Transform _transform;
		private float _uprootedAngularDrag;
		private float _uprootedMass;

		/// <summary>A root system initially has no root grown out; i.e., will not enroot to other objects.</summary>
		private void Start()
		{
			_source = GetComponent<AudioSource>();
			_transform = transform;
			_uprootedMass = GetComponent<Rigidbody>().mass;
			_uprootedAngularDrag = GetComponent<Rigidbody>().angularDrag;
			_root = (from c in GetComponents<CapsuleCollider>() where c.isTrigger select c).First();
			Assert.IsNotNull(_root);
			_root.enabled = false;
		}

		// private void Update()
		// {
		// 	// TODO do we need anything here?
		// }

		/// <summary>When the root collides with a "Ground" object we will enroot.</summary>
		/// <todo>Perhaps we should use a "Substrate" tag instead of "Ground".</todo>
		private void OnTriggerEnter(Collider other)
		{
			if (_rootLinkage == null && other.CompareTag("Ground"))
				Enroot(other.GetComponent<Rigidbody>());
		}

		/// <summary>Make sure when know when the roots no longer collides with the ground/substrate.</summary>
		private void OnTriggerExit(Collider other)
		{
			if (_ground && _ground.gameObject == other.gameObject)
			{
				_ground = null;
			}
		}

		/// <summary>Checks if the entity's roots are grown out and may collide with things.</summary>
		/// <returns><tt>true</tt> if the root may collide with objects, <tt>false</tt> otherwise</returns>
		internal bool HasRoots() => _root.enabled;

		/// <summary>Checks if the roots are anchored to the ground (or a substrate).</summary>
		/// <returns><tt>true</tt> if the roots are attached to something, <tt>false</tt> otherwise</returns>
		internal bool IsRooted() => _rootLinkage && _rootLinkage.connectedBody;

		/// <summary>Grows out the root tendrils and enables the collision-detecting for rooting.</summary>
		internal void GrowRoot()
		{
			// _rootTendrils = Instantiate(rootTendrils, _transform, false);
			// _rootTendrils.transform.localPosition = new Vector3(0, 0.25f, 0);
			// _rootTendrils.SetActive(true);
			_root.enabled = true;
		}

		/// <summary>Breaks off the current root and un-anchors from the rooted substrate.</summary>
		/// <todo>Trigger dissolve for the root left behind</todo>
		internal void Uproot()
		{
			if (_rootLinkage == null) return;
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().mass = _uprootedMass;
			GetComponent<Rigidbody>().angularDrag = _uprootedAngularDrag;
			GetComponent<Rigidbody>().useGravity = true;
			_rootTendrils.transform.parent = _rootLinkage.connectedBody.transform;
			Destroy(_rootLinkage);
			_rootLinkage = null;
			_root.enabled = false;
			_source.PlayOneShot(unlatch);
		}

		/// <summary>Anchors the roots to a <paramref name="substrate" />.</summary>
		/// <param name="substrate">a rigidbody to which the roots will anchor</param>
		private void Enroot(Rigidbody substrate)
		{
			_rootTendrils = Instantiate(rootTendrils, _transform, false);
			_rootTendrils.SetActive(true);

			_ground = substrate;
			GetComponent<Rigidbody>().mass = rootedMass;
			GetComponent<Rigidbody>().angularDrag = rootedAngularDrag;
			GetComponent<Rigidbody>().useGravity = false;
			_rootLinkage = gameObject.AddComponent<SpringJoint>();
			_rootLinkage.enableCollision = true;
			// \todo make these constants or fields?
			_rootLinkage.spring = RootSpring;
			_rootLinkage.damper = RootDamper;
			_rootLinkage.tolerance = RootTolerance;
			_rootLinkage.anchor = _root.center;
			_rootLinkage.connectedBody = _ground;
			_source.PlayOneShot(latch);
		}
	}
}
