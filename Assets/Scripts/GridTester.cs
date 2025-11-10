using UnityEngine;
using Domain.Gameplay.Models;
using Presentation.Gameplay.Views;

public class GridTester : MonoBehaviour
{
    [SerializeField]
    private GridView _gridView;

    private void Start()
    {
        Domain.Gameplay.Models.Grid grid = new Domain.Gameplay.Models.Grid(32, 32);
        this._gridView.Initialize(grid);

        // Тестовые клетки
        this._gridView.SetCellState(new GridPosition(5, 5), true);
        this._gridView.SetCellState(new GridPosition(10, 10), true);
        this._gridView.SetCellState(new GridPosition(15, 15), true);
    }
}
