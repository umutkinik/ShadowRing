using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;

namespace Golgehalka.Core
{
    /// Yerleştirme akışı:
    /// 1) UI'dan kahraman seç  2) boş node'a dokun/tıkla  3) altın yetiyorsa inşa et.
    /// Editor + mobil ikisinde de çalışır (mouse fallback).
    public class BuildManager : MonoBehaviour
    {
        [SerializeField] private GameObject towerBasePrefab; // Tower bileşenli taban

        private HeroDefinition selectedHero;

        public HeroDefinition SelectedHero => selectedHero;

        /// Kahraman seçim panelinden çağrılır.
        public void SelectHero(HeroDefinition hero) => selectedHero = hero;

        private void Update()
        {
            if (selectedHero == null) return;
            if (!TryGetTapPosition(out Vector2 screenPos)) return;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;

            var node = hit.collider.GetComponent<PlacementNode>();
            if (node == null || !node.IsEmpty) return;

            TryBuild(node);
        }

        /// Mobilde dokunuş, editor/masaüstünde sol tık.
        private static bool TryGetTapPosition(out Vector2 pos)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                pos = Input.GetTouch(0).position;
                return true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                pos = Input.mousePosition;
                return true;
            }
            pos = default;
            return false;
        }

        private void TryBuild(PlacementNode node)
        {
            int cost = selectedHero.tiers[0].cost;
            if (!EconomyManager.Instance.TrySpend(cost))
            {
                // UI: shop.not_enough_gold göster
                return;
            }
            var go = Instantiate(towerBasePrefab, node.transform.position, Quaternion.identity);
            var tower = go.GetComponent<Tower>();
            tower.Init(selectedHero);
            node.Occupant = tower;
            selectedHero = null; // yerleştirme sonrası seçim sıfırlanır
        }
    }
}
