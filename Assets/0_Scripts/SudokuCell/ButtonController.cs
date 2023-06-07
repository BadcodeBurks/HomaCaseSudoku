using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Sudoku
{
    public class ButtonController : MonoBehaviour
    {
        public RectShowUtility rectShowUtility;
        public Button button;
        public TextMeshProUGUI buttonText;
        
        public void InitButton(int number)
        {
            button.colors = ThemeManager.I.buttonColors;
            rectShowUtility.startHidden = true;
            rectShowUtility.Setup();
            if (number != 0)
            {
                buttonText.text = number.ToString();
                
                rectShowUtility.SetHideDir(RectShowUtility.OpDirection.Down);
            }
            else rectShowUtility.SetHideDir(RectShowUtility.OpDirection.Right);
            rectShowUtility.Init();
            button.onClick.AddListener(() => InputManager.I.OnSudokuButtonClick((uint)number));
        }

        public void Hide()
        {
            rectShowUtility.Show(false);
        }
        
        public void DestroyButton()
        {
            Destroy(gameObject);
        }

        public void Alert()
        {
            Vector2 startVelocity = (Vector2.up + (Vector2.right * Random.Range(-1f, 1f))) * 300;
            ParticleManager.SpawnParticle(ParticleManager.ParticleType.What, transform.position, Vector3.up * 50, Random.Range(40, 100), startVelocity);
        }
    }
}