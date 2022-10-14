using UnityEngine;

public class EmptyCell : Cell
{
    [SerializeField] Tower towerPf;
    [SerializeField] Gate gatePf;

    private Material _material;
    private Color _baseColor;

    protected bool _canBuy = true;
    protected bool _canWalk = true;

    public override bool CanBuy => _canBuy;
    public override bool CanWalk => _canWalk;   
    public Gate Gate { get; protected set; }
    public Tower Tower { get; protected set; }

    protected void Awake()
    {
        _material = GetComponentInChildren<Renderer>().material;
        _baseColor = _material.color;
    }

    public override void Select(Color color)
    {
        _material.color = color;
    }
    public override void UnSelect()
    {
        _material.color = _baseColor;
    }

    public void CreateGate()
    {
        if (!_canBuy) return;
        Gate = Instantiate(gatePf, transform);
        Gate.Cell = this;
        _canBuy = false;
    }
    public void CreateTower()
    {
        if (!_canBuy) return;
        Tower = Instantiate(towerPf, transform);
        _canBuy = false;
        _canWalk = false;
    }
    public ICreateTowerHandler TryCreateTower()
    {
        if (!_canBuy) return null;
        _canBuy = false;
        _canWalk = false;
        return new CreateTowerHandler(this);
    }
    private class CreateTowerHandler : ICreateTowerHandler
    {
        private readonly EmptyCell _owner;

        public CreateTowerHandler(EmptyCell owner)
        {
            _owner = owner;
        }

        public void Accept()
        {
            Cancel();
            _owner.CreateTower();
        }
        public void Cancel()
        {
            _owner._canBuy = true;
            _owner._canWalk = true;
        }
    }
}
public interface ICreateTowerHandler
{
    void Accept();
    void Cancel();
}
