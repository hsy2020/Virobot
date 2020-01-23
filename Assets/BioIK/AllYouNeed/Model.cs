using UnityEngine;
using System.Collections.Generic;

namespace BioIK {
	public class Model {
		public IKBody Body {get; private set;}

		private float OPX, OPY, OPZ;
		private float ORX, ORY, ORZ, ORW;
		private float OSX, OSY, OSZ;

		public Node[] Nodes = new Node[0];

		public MotionPtr[] Motions = new MotionPtr[0];
		public TipPtr[] Tips = new TipPtr[0];

		public Model(IKBody body) {
			Body = body;
			AddNode(Body.transform);
			BuildModel();
			UpdateState();
		}

		public void UpdateState() {
			UpdateOffset();
			Nodes[0].UpdateTransform();
		}

		public void ApplyConfiguration(float[] configuration) {
			Nodes[0].FeedForwardConfiguration(configuration);
		}

		public bool IsConverged(float[] configuration) {
			ApplyConfiguration(configuration);
			for(int i=0; i<Tips.Length; i++) {
				IKTip tip = Tips[i].Tip;
				Node node = Tips[i].Node;
				switch(tip.Objective.Type) {
					case IKTip.ObjectiveType.Position:
					if(node.ComputeTranslationalDistance(tip.TPX, tip.TPY, tip.TPZ) > tip.Objective.MaximumPositionError) {
						return false;
					}
					break;

					case IKTip.ObjectiveType.Orientation:
					if(node.ComputeRotationalDistance(tip.TRX, tip.TRY, tip.TRZ, tip.TRW) > tip.Objective.MaximumOrientationError) {
						return false;
					}
					break;

					case IKTip.ObjectiveType.Pose:
					if(node.ComputeTranslationalDistance(tip.TPX, tip.TPY, tip.TPZ) > tip.Objective.MaximumPositionError
					|| node.ComputeRotationalDistance(tip.TRX, tip.TRY, tip.TRZ, tip.TRW) > tip.Objective.MaximumOrientationError) {
						return false;
					}
					break;

					case IKTip.ObjectiveType.LookAt:
					if(node.ComputeDirectionalError(tip.TPX, tip.TPY, tip.TPZ, tip.Objective.Direction) > tip.Objective.MaximumDirectionError) {
						return false;
					}
					break;
				}
			}
			return true;
		}

		public JointMotion[] GetMotions() {
			JointMotion[] motions = new JointMotion[Motions.Length];
			for(int i=0; i<motions.Length; i++) {
				motions[i] = Motions[i].Motion;
			}
			return motions;
		}

		public float[] GetRandomConfiguration()  {
			float[] configuration = new float[Motions.Length];
			for(int i=0; i<configuration.Length; i++) {
				configuration[i] = Random.Range(Motions[i].Motion.GetLowerLimit(), Motions[i].Motion.GetUpperLimit());
			}
			return configuration;
		}

		public float[] GetCurrentConfiguration() {
			float[] configuration = new float[Motions.Length];
			for(int i=0; i<configuration.Length; i++) {
				configuration[i] = Motions[i].Motion.CurrentValue;
			}
			return configuration;
		}

		public float[] GetTargetConfiguration()  {
			float[] configuration = new float[Motions.Length];
			for(int i=0; i<configuration.Length; i++) {
				configuration[i] = Motions[i].Motion.GetTargetValue();
			}
			return configuration;
		}

		private void UpdateOffset() {
			if(Body.transform == Body.transform.root) {
				OPX = OPY = OPZ = ORX = ORY = ORZ = 0f;
				ORW = OSX = OSY = OSZ = 1f;
			} else {
				Vector3 p = Body.transform.parent.position;
				Quaternion r = Body.transform.parent.rotation;
				Vector3 s = Body.transform.parent.lossyScale;
				OPX = p.x; OPY = p.y; OPZ = p.z;
				ORX = r.x; ORY = r.y; ORZ = r.z; ORW = r.w;
				OSX = s.x; OSY = s.y; OSZ = s.z;
			}
		}

		private void BuildModel() {
			IKTip[] tips = FindTips(Body.transform, new List<IKTip>());
			for(int i=0; i<tips.Length; i++) {
				Chain chain = new Chain(Body.transform, tips[i].transform);
				for(int j=1; j<chain.Segments.Length; j++) {
					AddNode(chain.Segments[j]);
				}
			} 
		}

		private IKTip[] FindTips(Transform t, List<IKTip> tips) {
			IKTip tip = t.GetComponent<IKTip>();
			if(tip != null) {
				if(tip.isActiveAndEnabled) {
					tips.Add(tip);
				}
			}
			for(int i=0; i<t.childCount; i++) {
				FindTips(t.GetChild(i), tips);
			}
			return tips.ToArray();
		}

		private void AddNode(Transform segment) {
			if(FindNode(segment) == null) {
				KinematicJoint joint = segment.GetComponent<KinematicJoint>();
				MotionPtr[] motions = new MotionPtr[3];
				
				Node node = new Node(this, FindNode(segment.parent), segment, joint, motions);

				if(joint != null) {
					if(joint.GetDOF() == 0) {
						joint = null;
					} else {
						if(joint.XMotion.State != JointState.Fixed) {
							MotionPtr motionPtr = new MotionPtr(joint.XMotion, node, Motions.Length);
							System.Array.Resize(ref Motions, Motions.Length+1);
							Motions[Motions.Length-1] = motionPtr;
							motions[0] = motionPtr;
						}
						if(joint.YMotion.State != JointState.Fixed) {
							MotionPtr motionPtr = new MotionPtr(joint.YMotion, node, Motions.Length);
							System.Array.Resize(ref Motions, Motions.Length+1);
							Motions[Motions.Length-1] = motionPtr;
							motions[1] = motionPtr;
						}
						if(joint.ZMotion.State != JointState.Fixed) {
							MotionPtr motionPtr = new MotionPtr(joint.ZMotion, node, Motions.Length);
							System.Array.Resize(ref Motions, Motions.Length+1);
							Motions[Motions.Length-1] = motionPtr;
							motions[2] = motionPtr;
						}
					}
				}

				IKTip tip = segment.GetComponent<IKTip>();
				if(tip != null) {
					System.Array.Resize(ref Tips, Tips.Length+1);
					Tips[Tips.Length-1] = new TipPtr(segment.GetComponent<IKTip>(), node, Tips.Length);
				}

				System.Array.Resize(ref Nodes, Nodes.Length+1);
				Nodes[Nodes.Length-1] = node;
			}
		}

		public Node FindNode(Transform segment) {
			return System.Array.Find(
				Nodes,
				node => node.Segment == segment
			);
		}

		public class Node {
			public Model Model;
			public Node Parent;

			public Transform Segment;
			public Chain Chain;
			public KinematicJoint Joint;
			public MotionPtr[] Motions;

			public float HeuristicError;
			private float HeuristicInputs;
			private Node[] Childs = new Node[0];
			private float[] Values = new float[3];

			public float WPX, WPY, WPZ;
			public float WRX, WRY, WRZ, WRW;
			public float WSX, WSY, WSZ;

			private float LPX, LPY, LPZ;
			private float LRX, LRY, LRZ, LRW;
			private float LSX, LSY, LSZ;

			private float num1, num2, num3, num4, num5, num6, num7, num8, num9, num10, num11, num12;

			public Node(Model model, Node parent, Transform segment, KinematicJoint joint, MotionPtr[] motions) {
				Model = model;
				Parent = parent;
				if(Parent != null) {
					Parent.AddChild(this);
				}
				Segment = segment;
				Chain = new Chain(model.Body.transform, segment);
				Joint = joint;
				Motions = motions;

				HeuristicInputs = 0f;
				HeuristicError = 0f;
			}

			public void AddChild(Node child) {
				if(System.Array.Find(Childs, x => x == child) == null) {
					System.Array.Resize(ref Childs, Childs.Length+1);
					Childs[Childs.Length-1] = child;
				} else {
					Debug.LogWarning("Refused to add child " + child.Segment.name + ". This should not have happened.");
				}
			}

			public float ComputeTranslationalDistance(float targetPositionX, float targetPositionY, float targetPositionZ) {
				//Euclidean Distance: ||A-B||
				float dX = targetPositionX-WPX;
				float dY = targetPositionY-WPY;
				float dZ = targetPositionZ-WPZ;
				return Mathf.Sqrt(dX*dX + dY*dY + dZ*dZ);
			}

			public float ComputeRotationalDistance(float targetRotationX, float targetRotationY, float targetRotationZ, float targetRotationW) {
				//Quaternion Angle: 2*ACos(|AxB|)
				return 2f * Mathf.Acos(Mathf.Min(1f, Mathf.Abs(WRX*targetRotationX + WRY*targetRotationY + WRZ*targetRotationZ + WRW*targetRotationW)));
			}

			public float ComputeDirectionalError(float targetPositionX, float targetPositionY, float targetPositionZ, Vector3 axis) {
				return
				Mathf.Deg2Rad*Vector3.Angle(
					new Quaternion(WRX, WRY, WRZ, WRW)*axis,
					new Vector3(targetPositionX-WPX, targetPositionY-WPY, targetPositionZ-WPZ)
				);
			}

			public float ComputeAngularScale() {
				if(Chain.Joints.Length == 0) {
					return 1f;
				} else {
					Vector3 connection = Chain.Joints[0].ConnectionInWorldSpace;
					float dX = WPX-connection.x;
					float dY = WPY-connection.y;
					float dZ = WPZ-connection.z;
					return Mathf.Sqrt(Chain.Length*Mathf.Sqrt(dX*dX + dY*dY + dZ*dZ)) / Mathf.PI;
				}
			}

			public Vector3 GetWorldPosition() {
				return new Vector3(WPX, WPY, WPZ);
			}

			public Quaternion GetWorldRotation() {
				return new Quaternion(WRX, WRY, WRZ, WRW);
			}

			public Vector3 GetWorldScale() {
				return new Vector3(WSX, WSY, WSZ);
			}

			public void FeedForwardConfiguration(float[] configuration, bool updateWorld = false) {
				HeuristicInputs = 0f;
				HeuristicError = 0f;

				bool updateLocal = false;
				if(Motions[0] != null) {
					if(configuration[Motions[0].Index] != Values[0]) {
						Values[0] = configuration[Motions[0].Index];
						updateLocal = true;
						updateWorld = true;
					}
				}
				if(Motions[1] != null) {
					if(configuration[Motions[1].Index] != Values[1]) {
						Values[1] = configuration[Motions[1].Index];
						updateLocal = true;
						updateWorld = true;
					}
				}
				if(Motions[2] != null) {
					if(configuration[Motions[2].Index] != Values[2]) {
						Values[2] = configuration[Motions[2].Index];
						updateLocal = true;
						updateWorld = true;
					}
				}
				
				if(updateLocal) {
					Joint.ComputeTransformation(Values[0], Values[1], Values[2], out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
				}

				if(updateWorld) {
					if(Parent == null) {
						TransformCoordinateSystem(
							ref Model.OPX, ref Model.OPY, ref Model.OPZ, ref Model.ORX, ref Model.ORY, ref Model.ORZ, ref Model.ORW, ref Model.OSX, ref Model.OSY, ref Model.OSZ,
							ref LPX, ref LPY, ref LPZ, ref LRX, ref LRY, ref LRZ, ref LRW, ref LSX, ref LSY, ref LSZ,
							out WPX, out WPY, out WPZ, out WRX, out WRY, out WRZ, out WRW, out WSX, out WSY, out WSZ
						);
					} else {
						TransformCoordinateSystem(
							ref Parent.WPX, ref Parent.WPY, ref Parent.WPZ, ref Parent.WRX, ref Parent.WRY, ref Parent.WRZ, ref Parent.WRW, ref Parent.WSX, ref Parent.WSY, ref Parent.WSZ,
							ref LPX, ref LPY, ref LPZ, ref LRX, ref LRY, ref LRZ, ref LRW, ref LSX, ref LSY, ref LSZ,
							out WPX, out WPY, out WPZ, out WRX, out WRY, out WRZ, out WRW, out WSX, out WSY, out WSZ
						);
					}
				}

				for(int i=0; i<Childs.Length; i++) {
					Childs[i].FeedForwardConfiguration(configuration, updateWorld);
				}
			}

			public void BackpropagateHeuristicError(float fitness) {
				HeuristicInputs += 1f;
				HeuristicError = HeuristicError * (HeuristicInputs-1f)/HeuristicInputs + fitness/HeuristicInputs;
				if(Parent != null) {
					Parent.BackpropagateHeuristicError(fitness);
				}
			}

			public void UpdateTransform() {
				//Local
				if(Joint == null) {
					Vector3 lp = Segment.localPosition;
					Quaternion lr = Segment.localRotation;
					LPX = lp.x;
					LPY = lp.y;
					LPZ = lp.z;
					LRX = lr.x;
					LRY = lr.y;
					LRZ = lr.z;
					LRW = lr.w;
				} else {
					Values[0] = Joint.XMotion.GetTargetValue();
					Values[1] = Joint.YMotion.GetTargetValue();
					Values[2] = Joint.ZMotion.GetTargetValue();
					Joint.ComputeTransformation(Values[0], Values[1], Values[2], out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
				}
				Vector3 ls = Segment.localScale;
				LSX = ls.x;
				LSY = ls.y;
				LSZ = ls.z;

				//World
				if(Parent == null) {
					TransformCoordinateSystem(
						ref Model.OPX, ref Model.OPY, ref Model.OPZ, ref Model.ORX, ref Model.ORY, ref Model.ORZ, ref Model.ORW, ref Model.OSX, ref Model.OSY, ref Model.OSZ,
						ref LPX, ref LPY, ref LPZ, ref LRX, ref LRY, ref LRZ, ref LRW, ref LSX, ref LSY, ref LSZ,
						out WPX, out WPY, out WPZ, out WRX, out WRY, out WRZ, out WRW, out WSX, out WSY, out WSZ
					);
				} else {
					TransformCoordinateSystem(
						ref Parent.WPX, ref Parent.WPY, ref Parent.WPZ, ref Parent.WRX, ref Parent.WRY, ref Parent.WRZ, ref Parent.WRW, ref Parent.WSX, ref Parent.WSY, ref Parent.WSZ,
						ref LPX, ref LPY, ref LPZ, ref LRX, ref LRY, ref LRZ, ref LRW, ref LSX, ref LSY, ref LSZ,
						out WPX, out WPY, out WPZ, out WRX, out WRY, out WRZ, out WRW, out WSX, out WSY, out WSZ
					);
				}

				//Feed Forward
				for(int i=0; i<Childs.Length; i++) {
					Childs[i].UpdateTransform();
				}
			}

			private void TransformCoordinateSystem(
				ref float tpX, ref float tpY, ref float tpZ, ref float trX, ref float trY, ref float trZ, ref float trW, ref float tsX, ref float tsY, ref float tsZ,
				ref float lpX, ref float lpY, ref float lpZ, ref float lrX, ref float lrY, ref float lrZ, ref float lrW, ref float lsX, ref float lsY, ref float lsZ,
				out float rpX, out float rpY, out float rpZ, out float rrX, out float rrY, out float rrZ, out float rrW, out float rsX, out float rsY, out float rsZ
			) {
				//Quaternion * Vector
				rpX = tpX + (1f - 2f * (trY * trY + trZ * trZ)) * lpX * tsX + 2f * (trX * trY - trW * trZ) * lpY * tsY + 2f * (trX * trZ + trW * trY) * lpZ * tsZ;
				rpY = tpY + 2f * (trX * trY + trW * trZ) * lpX * tsX + (1f - 2f * (trX * trX + trZ * trZ)) * lpY * tsY + 2f * (trY * trZ - trW * trX) * lpZ * tsZ;
				rpZ = tpZ + 2f * (trX * trZ - trW * trY) * lpX * tsX + 2f * (trY * trZ + trW * trX) * lpY * tsY + (1f - 2f * (trX * trX + trY * trY)) * lpZ * tsZ;

				//Quaternion * Quaternion
				rrX = trX * lrW + trY * lrZ - trZ * lrY + trW * lrX;
				rrY = -trX * lrZ + trY * lrW + trZ * lrX + trW * lrY;
				rrZ = trX * lrY - trY * lrX + trZ * lrW + trW * lrZ;
				rrW = -trX * lrX - trY * lrY - trZ * lrZ + trW * lrW;

				//Scale Vector
				rsX = tsX * lsX;
				rsY = tsY * lsY;
				rsZ = tsZ * lsZ;
			}
		}

		public class TipPtr {
			public IKTip Tip;
			public Node Node;
			public int Index;
			public TipPtr(IKTip tip, Node node, int index) {
				Tip = tip;
				Node = node;
				Index = index;
			}
		}

		public class MotionPtr {
			public JointMotion Motion;
			public Node Node;
			public int Index;
			public MotionPtr(JointMotion motion, Node node, int index) {
				Motion = motion;
				Node = node;
				Index = index;
			}
		}
	}
}