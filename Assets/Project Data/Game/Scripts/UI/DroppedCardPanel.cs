using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.Upgrades;
using YG;

namespace Watermelon.SquadShooter
{
    public class DroppedCardPanel : MonoBehaviour
    {
        private const string CARDS_TEXT = "{0}/{1}";

        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Image weaponPreviewImage;
        [SerializeField] Image weaponBackgroundImage;
        [SerializeField] TextMeshProUGUI rarityText;

        [SerializeField] GameObject newRibbonObject;

        [Space]
        [SerializeField] GameObject progressPanelObject;
        [SerializeField] GameObject progressFillbarObject;
        [SerializeField] SlicedFilledImage progressFillbarImage;
        [SerializeField] TextMeshProUGUI progressFillbarText;
        [SerializeField] GameObject progressEquipButtonObject;
        [SerializeField] GameObject progressEquipedObject;

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup => canvasGroup;

        private WeaponData weaponData;
        private RarityData rarityData;
        private BaseWeaponUpgrade weaponUpgrade;

        private int currentCardsAmount;

        public void Initialise(WeaponType weaponType)
        {
            canvasGroup = GetComponent<CanvasGroup>();

            weaponData = WeaponsController.GetWeaponData(weaponType);
            rarityData = WeaponsController.GetRarityData(weaponData.Rarity);
            weaponUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);

            currentCardsAmount = weaponData.CardsAmount;

            switch (YandexGame.lang)
            {
                case "ru":
                    titleText.text = weaponData.RussianName;
                    break;
                case "en":
                    titleText.text = weaponData.EnglishName;
                    break;
                default:
                    titleText.text = weaponData.EnglishName;
                    break;

            }

            weaponPreviewImage.sprite = weaponData.Icon;
            if (YandexGame.lang == "ru")
            {
                string rarityName = rarityData.Name;
                switch (rarityName)
                {
                    case "COMMON":
                        rarityText.text = "ÎÁÛ×ÍÛÉ";
                        break;
                    case "RARE":
                        rarityText.text = "ÐÅÄÊÈÉ";
                        break;
                    case "EPIC":
                        rarityText.text = "ÝÏÈ×ÍÛÉ";
                        break;
                    case "LEGENDARY":
                        rarityText.text = "ËÅÃÅÍÄÀÐÍÛÉ";
                        break;
                    default:
                        rarityText.text = rarityData.Name;
                        break;
                }
            }
            else rarityText.text = rarityData.Name;
            rarityText.color = rarityData.TextColor;

            weaponBackgroundImage.color = rarityData.MainColor;

            progressPanelObject.SetActive(false);
        }

        public void OnDisplayed()
        {
            int target = weaponUpgrade.Upgrades[1].Price;

            progressPanelObject.SetActive(true);
            progressFillbarObject.SetActive(true);

            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(false);

            progressFillbarText.text = string.Format(CARDS_TEXT, currentCardsAmount, target);

            progressPanelObject.transform.localScale = Vector3.one * 0.8f;
            progressPanelObject.transform.DOScale(Vector3.one, 0.15f).SetEasing(Ease.Type.BackOut);

            progressFillbarImage.fillAmount = 0.0f;
            progressFillbarImage.DOFillAmount((float)currentCardsAmount / target, 0.4f, 0.1f).OnComplete(delegate
            {
                if (currentCardsAmount >= target)
                {
                    Tween.DelayedCall(0.5f, delegate
                    {
                        progressFillbarObject.SetActive(false);

                        progressEquipButtonObject.SetActive(true);
                        progressEquipButtonObject.transform.localScale = Vector3.one * 0.7f;
                        progressEquipButtonObject.transform.DOScale(Vector3.one, 0.25f).SetEasing(Ease.Type.BackOut);
                    });
                }
            });
        }

        public void OnEquipButtonClicked()
        {
            WeaponsController.SelectWeapon(weaponData.Type);

            progressEquipButtonObject.SetActive(false);
            progressEquipedObject.SetActive(true);

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
    }
}