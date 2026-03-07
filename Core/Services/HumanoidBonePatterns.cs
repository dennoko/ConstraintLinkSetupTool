using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tiloop.ConstraintLinkSetupTool.Core.Services
{
    /// <summary>
    /// ヒューマノイドボーンの名前パターン辞書
    /// MAのHeuristicBoneMapperを参考に、各ボーンに対する複数の名前バリエーションを定義
    /// </summary>
    public static class HumanoidBonePatterns
    {
        /// <summary>
        /// HumanBodyBonesのインデックスに対応するボーン名パターン配列
        /// </summary>
        public static readonly string[][] BoneNamePatterns = new[]
        {
            // 0: Hips
            new[] { "Hips", "Hip", "pelvis", "Pelvis", "J_Bip_C_Hips" },
            
            // 1: LeftUpperLeg
            new[] {
                "LeftUpperLeg", "UpperLeg_Left", "UpperLeg_L", "Leg_Left", "Leg_L", 
                "ULeg_L", "Left leg", "LeftUpLeg", "UpLeg.L", "Thigh_L", "Thigh.L",
                "J_Bip_L_UpperLeg", "左もも", "左太腿"
            },
            
            // 2: RightUpperLeg
            new[] {
                "RightUpperLeg", "UpperLeg_Right", "UpperLeg_R", "Leg_Right", "Leg_R",
                "ULeg_R", "Right leg", "RightUpLeg", "UpLeg.R", "Thigh_R", "Thigh.R",
                "J_Bip_R_UpperLeg", "右もも", "右太腿"
            },
            
            // 3: LeftLowerLeg
            new[] {
                "LeftLowerLeg", "LowerLeg_Left", "LowerLeg_L", "Knee_Left", "Knee_L",
                "LLeg_L", "Left knee", "LeftLeg", "leg_L", "shin.L", "Shin_L",
                "J_Bip_L_LowerLeg", "左すね", "左下腿"
            },
            
            // 4: RightLowerLeg
            new[] {
                "RightLowerLeg", "LowerLeg_Right", "LowerLeg_R", "Knee_Right", "Knee_R",
                "LLeg_R", "Right knee", "RightLeg", "leg_R", "shin.R", "Shin_R",
                "J_Bip_R_LowerLeg", "右すね", "右下腿"
            },
            
            // 5: LeftFoot
            new[] {
                "LeftFoot", "Foot_Left", "Foot_L", "Ankle_L", "Foot.L.001", "Left ankle",
                "heel.L", "J_Bip_L_Foot", "左足首", "左足"
            },
            
            // 6: RightFoot
            new[] {
                "RightFoot", "Foot_Right", "Foot_R", "Ankle_R", "Foot.R.001", "Right ankle",
                "heel.R", "J_Bip_R_Foot", "右足首", "右足"
            },
            
            // 7: Spine
            new[] { "Spine", "spine01", "Spine1", "J_Bip_C_Spine", "背骨" },
            
            // 8: Chest
            new[] { "Chest", "Bust", "spine02", "Spine2", "J_Bip_C_Chest", "胸" },
            
            // 9: Neck
            new[] { "Neck", "J_Bip_C_Neck", "首" },
            
            // 10: Head
            new[] { "Head", "J_Bip_C_Head", "頭" },
            
            // 11: LeftShoulder
            new[] {
                "LeftShoulder", "Shoulder_Left", "Shoulder_L", "L_Shoulder",
                "J_Bip_L_Shoulder", "左肩", "左鎖骨"
            },
            
            // 12: RightShoulder
            new[] {
                "RightShoulder", "Shoulder_Right", "Shoulder_R", "R_Shoulder",
                "J_Bip_R_Shoulder", "右肩", "右鎖骨"
            },
            
            // 13: LeftUpperArm
            new[] {
                "LeftUpperArm", "UpperArm_Left", "UpperArm_L", "Arm_Left", "Arm_L",
                "UArm_L", "Left arm", "UpperLeftArm", "J_Bip_L_UpperArm", "左上腕"
            },
            
            // 14: RightUpperArm
            new[] {
                "RightUpperArm", "UpperArm_Right", "UpperArm_R", "Arm_Right", "Arm_R",
                "UArm_R", "Right arm", "UpperRightArm", "J_Bip_R_UpperArm", "右上腕"
            },
            
            // 15: LeftLowerArm
            new[] {
                "LeftLowerArm", "LowerArm_Left", "LowerArm_L", "LArm_L", "Left elbow",
                "LeftForeArm", "Elbow_L", "forearm_L", "ForArm_L", "ForeArm.L",
                "J_Bip_L_LowerArm", "左前腕"
            },
            
            // 16: RightLowerArm
            new[] {
                "RightLowerArm", "LowerArm_Right", "LowerArm_R", "LArm_R", "Right elbow",
                "RightForeArm", "Elbow_R", "forearm_R", "ForArm_R", "ForeArm.R",
                "J_Bip_R_LowerArm", "右前腕"
            },
            
            // 17: LeftHand
            new[] {
                "LeftHand", "Hand_Left", "Hand_L", "Left wrist", "Wrist_L",
                "J_Bip_L_Hand", "左手"
            },
            
            // 18: RightHand
            new[] {
                "RightHand", "Hand_Right", "Hand_R", "Right wrist", "Wrist_R",
                "J_Bip_R_Hand", "右手"
            },
            
            // 19: LeftToes
            new[] {
                "LeftToes", "Toes_Left", "Toe_Left", "ToeIK_L", "Toes_L", "Toe_L",
                "Foot.L.002", "Left Toe", "LeftToeBase", "J_Bip_L_ToeBase", "左つま先"
            },
            
            // 20: RightToes
            new[] {
                "RightToes", "Toes_Right", "Toe_Right", "ToeIK_R", "Toes_R", "Toe_R",
                "Foot.R.002", "Right Toe", "RightToeBase", "J_Bip_R_ToeBase", "右つま先"
            },
            
            // 21: LeftEye
            new[] { "LeftEye", "Eye_Left", "Eye_L", "J_Adj_L_FaceEye", "左目" },
            
            // 22: RightEye
            new[] { "RightEye", "Eye_Right", "Eye_R", "J_Adj_R_FaceEye", "右目" },
            
            // 23: Jaw
            new[] { "Jaw", "J_Adj_C_FaceJaw", "顎" },
            
            // 24-38: Left Hand Fingers
            new[] { "LeftThumbProximal", "ProximalThumb_Left", "ProximalThumb_L", "Thumb1_L", "ThumbFinger1_L", "LeftHandThumb1", "Thumb Proximal.L", "Thumb1_L", "finger01_01_L", "J_Bip_L_Thumb1" },
            new[] { "LeftThumbIntermediate", "IntermediateThumb_Left", "IntermediateThumb_L", "Thumb2_L", "ThumbFinger2_L", "LeftHandThumb2", "Thumb Intermediate.L", "Thumb2_L", "finger01_02_L", "J_Bip_L_Thumb2" },
            new[] { "LeftThumbDistal", "DistalThumb_Left", "DistalThumb_L", "Thumb3_L", "ThumbFinger3_L", "LeftHandThumb3", "Thumb Distal.L", "Thumb3_L", "finger01_03_L", "J_Bip_L_Thumb3" },
            
            new[] { "LeftIndexProximal", "ProximalIndex_Left", "ProximalIndex_L", "Index1_L", "IndexFinger1_L", "LeftHandIndex1", "Index Proximal.L", "finger02_01_L", "f_index.01.L", "J_Bip_L_Index1" },
            new[] { "LeftIndexIntermediate", "IntermediateIndex_Left", "IntermediateIndex_L", "Index2_L", "IndexFinger2_L", "LeftHandIndex2", "Index Intermediate.L", "finger02_02_L", "f_index.02.L", "J_Bip_L_Index2" },
            new[] { "LeftIndexDistal", "DistalIndex_Left", "DistalIndex_L", "Index3_L", "IndexFinger3_L", "LeftHandIndex3", "Index Distal.L", "finger02_03_L", "f_index.03.L", "J_Bip_L_Index3" },
            
            new[] { "LeftMiddleProximal", "ProximalMiddle_Left", "ProximalMiddle_L", "Middle1_L", "MiddleFinger1_L", "LeftHandMiddle1", "Middle Proximal.L", "finger03_01_L", "f_middle.01.L", "J_Bip_L_Middle1" },
            new[] { "LeftMiddleIntermediate", "IntermediateMiddle_Left", "IntermediateMiddle_L", "Middle2_L", "MiddleFinger2_L", "LeftHandMiddle2", "Middle Intermediate.L", "finger03_02_L", "f_middle.02.L", "J_Bip_L_Middle2" },
            new[] { "LeftMiddleDistal", "DistalMiddle_Left", "DistalMiddle_L", "Middle3_L", "MiddleFinger3_L", "LeftHandMiddle3", "Middle Distal.L", "finger03_03_L", "f_middle.03.L", "J_Bip_L_Middle3" },
            
            new[] { "LeftRingProximal", "ProximalRing_Left", "ProximalRing_L", "Ring1_L", "RingFinger1_L", "LeftHandRing1", "Ring Proximal.L", "finger04_01_L", "f_ring.01.L", "J_Bip_L_Ring1" },
            new[] { "LeftRingIntermediate", "IntermediateRing_Left", "IntermediateRing_L", "Ring2_L", "RingFinger2_L", "LeftHandRing2", "Ring Intermediate.L", "finger04_02_L", "f_ring.02.L", "J_Bip_L_Ring2" },
            new[] { "LeftRingDistal", "DistalRing_Left", "DistalRing_L", "Ring3_L", "RingFinger3_L", "LeftHandRing3", "Ring Distal.L", "finger04_03_L", "f_ring.03.L", "J_Bip_L_Ring3" },
            
            new[] { "LeftLittleProximal", "ProximalLittle_Left", "ProximalLittle_L", "Little1_L", "LittleFinger1_L", "LeftHandPinky1", "Little Proximal.L", "finger05_01_L", "f_pinky.01.L", "J_Bip_L_Little1" },
            new[] { "LeftLittleIntermediate", "IntermediateLittle_Left", "IntermediateLittle_L", "Little2_L", "LittleFinger2_L", "LeftHandPinky2", "Little Intermediate.L", "finger05_02_L", "f_pinky.02.L", "J_Bip_L_Little2" },
            new[] { "LeftLittleDistal", "DistalLittle_Left", "DistalLittle_L", "Little3_L", "LittleFinger3_L", "LeftHandPinky3", "Little Distal.L", "finger05_03_L", "f_pinky.03.L", "J_Bip_L_Little3" },
            
            // 39-53: Right Hand Fingers
            new[] { "RightThumbProximal", "ProximalThumb_Right", "ProximalThumb_R", "Thumb1_R", "ThumbFinger1_R", "RightHandThumb1", "Thumb Proximal.R", "Thumb1_R", "finger01_01_R", "J_Bip_R_Thumb1" },
            new[] { "RightThumbIntermediate", "IntermediateThumb_Right", "IntermediateThumb_R", "Thumb2_R", "ThumbFinger2_R", "RightHandThumb2", "Thumb Intermediate.R", "Thumb2_R", "finger01_02_R", "J_Bip_R_Thumb2" },
            new[] { "RightThumbDistal", "DistalThumb_Right", "DistalThumb_R", "Thumb3_R", "ThumbFinger3_R", "RightHandThumb3", "Thumb Distal.R", "Thumb3_R", "finger01_03_R", "J_Bip_R_Thumb3" },
            
            new[] { "RightIndexProximal", "ProximalIndex_Right", "ProximalIndex_R", "Index1_R", "IndexFinger1_R", "RightHandIndex1", "Index Proximal.R", "finger02_01_R", "f_index.01.R", "J_Bip_R_Index1" },
            new[] { "RightIndexIntermediate", "IntermediateIndex_Right", "IntermediateIndex_R", "Index2_R", "IndexFinger2_R", "RightHandIndex2", "Index Intermediate.R", "finger02_02_R", "f_index.02.R", "J_Bip_R_Index2" },
            new[] { "RightIndexDistal", "DistalIndex_Right", "DistalIndex_R", "Index3_R", "IndexFinger3_R", "RightHandIndex3", "Index Distal.R", "finger02_03_R", "f_index.03.R", "J_Bip_R_Index3" },
            
            new[] { "RightMiddleProximal", "ProximalMiddle_Right", "ProximalMiddle_R", "Middle1_R", "MiddleFinger1_R", "RightHandMiddle1", "Middle Proximal.R", "finger03_01_R", "f_middle.01.R", "J_Bip_R_Middle1" },
            new[] { "RightMiddleIntermediate", "IntermediateMiddle_Right", "IntermediateMiddle_R", "Middle2_R", "MiddleFinger2_R", "RightHandMiddle2", "Middle Intermediate.R", "finger03_02_R", "f_middle.02.R", "J_Bip_R_Middle2" },
            new[] { "RightMiddleDistal", "DistalMiddle_Right", "DistalMiddle_R", "Middle3_R", "MiddleFinger3_R", "RightHandMiddle3", "Middle Distal.R", "finger03_03_R", "f_middle.03.R", "J_Bip_R_Middle3" },
            
            new[] { "RightRingProximal", "ProximalRing_Right", "ProximalRing_R", "Ring1_R", "RingFinger1_R", "RightHandRing1", "Ring Proximal.R", "finger04_01_R", "f_ring.01.R", "J_Bip_R_Ring1" },
            new[] { "RightRingIntermediate", "IntermediateRing_Right", "IntermediateRing_R", "Ring2_R", "RingFinger2_R", "RightHandRing2", "Ring Intermediate.R", "finger04_02_R", "f_ring.02.R", "J_Bip_R_Ring2" },
            new[] { "RightRingDistal", "DistalRing_Right", "DistalRing_R", "Ring3_R", "RingFinger3_R", "RightHandRing3", "Ring Distal.R", "finger04_03_R", "f_ring.03.R", "J_Bip_R_Ring3" },
            
            new[] { "RightLittleProximal", "ProximalLittle_Right", "ProximalLittle_R", "Little1_R", "LittleFinger1_R", "RightHandPinky1", "Little Proximal.R", "finger05_01_R", "f_pinky.01.R", "J_Bip_R_Little1" },
            new[] { "RightLittleIntermediate", "IntermediateLittle_Right", "IntermediateLittle_R", "Little2_R", "LittleFinger2_R", "RightHandPinky2", "Little Intermediate.R", "finger05_02_R", "f_pinky.02.R", "J_Bip_R_Little2" },
            new[] { "RightLittleDistal", "DistalLittle_Right", "DistalLittle_R", "Little3_R", "LittleFinger3_R", "RightHandPinky3", "Little Distal.R", "finger05_03_R", "f_pinky.03.R", "J_Bip_R_Little3" },
            
            // 54: UpperChest
            new[] { "UpperChest", "UChest", "spine03", "Spine3", "J_Bip_C_UpperChest", "上胸" },
        };

        /// <summary>
        /// 正規化されたボーン名からHumanBodyBonesへのマッピング
        /// </summary>
        public static readonly Dictionary<string, List<HumanBodyBones>> NameToBoneMap;

        /// <summary>
        /// HumanBodyBonesから可能な名前パターンへのマッピング
        /// </summary>
        public static readonly Dictionary<HumanBodyBones, List<string>> BoneToNameMap;

        /// <summary>
        /// 全ての正規化されたボーン名のセット
        /// </summary>
        public static readonly HashSet<string> AllNormalizedBoneNames;

        static HumanoidBonePatterns()
        {
            NameToBoneMap = new Dictionary<string, List<HumanBodyBones>>();
            BoneToNameMap = new Dictionary<HumanBodyBones, List<string>>();

            for (int i = 0; i < BoneNamePatterns.Length; i++)
            {
                var bone = (HumanBodyBones)i;
                var names = new List<string>();

                foreach (var name in BoneNamePatterns[i])
                {
                    var normalizedName = BoneNameMatcher.NormalizeBoneName(name);
                    
                    if (!NameToBoneMap.TryGetValue(normalizedName, out var boneList))
                    {
                        boneList = new List<HumanBodyBones>();
                        NameToBoneMap[normalizedName] = boneList;
                    }
                    if (!boneList.Contains(bone))
                    {
                        boneList.Add(bone);
                    }

                    if (!names.Contains(normalizedName))
                    {
                        names.Add(normalizedName);
                    }
                }

                BoneToNameMap[bone] = names;
            }

            AllNormalizedBoneNames = new HashSet<string>(
                BoneNamePatterns
                    .SelectMany(x => x)
                    .Select(BoneNameMatcher.NormalizeBoneName)
            );
        }
    }
}
