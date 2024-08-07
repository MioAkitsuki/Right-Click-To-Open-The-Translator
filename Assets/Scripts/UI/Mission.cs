using System.Collections;
using System.Collections.Generic;
using DataSystem;
using QFramework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    [RequireComponent(typeof(Button))]
    public class Mission : MonoBehaviour
    {
        private Button m_button;
        public MissionData data { get; private set; }

        private void Start()
        {
            m_button = this.gameObject.GetComponent<Button>();
            m_button.onClick.AddListener(() =>
            {
                MissionController.Instance.ChangeDetailMissionInfo(this.data);
                MissionController.Instance.SetHighlightImage(transform.Find("Icon").GetComponent<Image>());
            });
            TypeEventSystem.Global.Register<MissionController.MissionListRefreshedEvent>(e =>
            {
                if (this.data.Id.Equals(e.id))
                {
                    MissionController.Instance.SetHighlightImage(transform.Find("Icon").GetComponent<Image>());
                }
            });
        }
        public static GameObject Initialize(MissionData vardata, Transform parent)
        {
            var temp = Instantiate(MissionController.Instance.prefab, parent);
            temp.GetComponent<Mission>().data = vardata;
            temp.transform.Find("Name").GetComponent<TextMeshProUGUI>().SetText(vardata.Name);
            return temp;
        }
    }
}
