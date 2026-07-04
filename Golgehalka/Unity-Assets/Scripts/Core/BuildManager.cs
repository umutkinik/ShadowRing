using System;
using Golgehalka.Combat;
using Golgehalka.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Golgehalka.Core
{
    /// Dokunuş yönlendirme:
    ///   kahraman seçiliyken boş platforma dokun → inşa et
    ///   kahraman seçili değilken kuleye dokun → OnTowerTapped (yükseltme paneli açılır)
    /// Editor + mobil ikisinde de çalışır; UI üzerindeki dokunuşlar yok sayılır.
    public class BuildManager : MonoBehaviour
    {
        [SerializeField] private GameObject towerBasePrefab; // yedek genel taban

        /// MatchHUD panel açmak için dinler.
        public static event Action<Tower> OnTowerTapped;

        private HeroDefinition selectedHero;
        public HeroDefinition SelectedHero => selectedHero;

        public void SelectHero(HeroDefinition hero) => selectedHero = hero;
        public void ClearSelection() => selectedHero = null;

        private void Update()
        {
            if (!TryGetTapPosition(out Vector2 screenPos)) return;
            if (IsPointerOverUI()) return; // HUD butonlarına dokunuş dünyaya geçmez

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return;

            // 1) İnşa: kahraman seçili + boş platform
            var node = hit.collider.GetComponent<PlacementNode>();
            if (node != null)
            {
                if (selectedHero != null && node.IsEmpty) TryBuild(node);
                else if (!node.IsEmpty) OnTowerTapped?.Invoke(node.Occupant);
                return;
            }

            // 2) Seçim: kurulu kuleye (veya model çocuğuna) dokunuş
            var tower = hit.collider.GetComponentInParent<Tower>();
            if (tower != null) OnTowerTapped?.Invoke(tower);
        }

        private static bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return EventSystem.current.IsPointerOverGameObject();
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
            var prefab = selectedHero.towerPrefab != null ? selectedHero.towerPrefab : towerBasePrefab;
            var go = Instantiate(prefab, node.transform.position, Quaternion.identity);
            var tower = go.GetComponent<Tower>();
            tower.Init(selectedHero);
            tower.Node = node;
            node.Occupant = tower;
            var nodeRenderer = node.GetComponent<Renderer>();
            if (nodeRenderer != null) nodeRenderer.enabled = false; // platform kulenin altında kalmasın
            selectedHero = null; // yerleştirme sonrası seçim sıfırlanır
        }
    }
}
