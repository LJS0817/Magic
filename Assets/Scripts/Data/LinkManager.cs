using Magic.Data;
using Magic.Drawing;
using Magic.Inventory;
using Magic.Upgrade;
using UnityEngine;

namespace Magic.Common
{
    public class LinkManager : MonoBehaviour
    {
        [SerializeField] DrawingDatabase _db;
        [SerializeField] UpgradeManager _upgrade;
        [SerializeField] PlayerDataManager _data;
        [SerializeField] InventoryManager _inventory;

        void Awake()
        {
            if(!_data.IsInit)
            {
                _db.InitInstance();
                _upgrade.InitInstance();
                _data.InitInstance();
                _inventory.InitInstance();
                DontDestroyOnLoad(gameObject);
            } else
            {
                Destroy(gameObject);
            }
        }
    }
}
