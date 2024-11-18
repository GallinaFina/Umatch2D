using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator instance;
    public static ServiceLocator Instance => instance;

    public TurnManager TurnManager { get; private set; }
    public CombatManager CombatManager { get; private set; }
    public MovementUI MovementUI { get; private set; }
    public HandDisplay HandDisplay { get; private set; }
    public EffectManager EffectManager { get; private set; }
    public Board Board { get; private set; }
    public Game GameManager { get; private set; }
    public DeckManager DeckManager { get; private set; }
    public CombatUI CombatUI { get; private set; }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeServices()
    {
        TurnManager = FindFirstObjectByType<TurnManager>();
        CombatManager = FindFirstObjectByType<CombatManager>();
        MovementUI = FindFirstObjectByType<MovementUI>();
        HandDisplay = FindFirstObjectByType<HandDisplay>();
        EffectManager = FindFirstObjectByType<EffectManager>();
        Board = FindFirstObjectByType<Board>();
        GameManager = FindFirstObjectByType<Game>();
        DeckManager = FindFirstObjectByType<DeckManager>();
        CombatUI = FindFirstObjectByType<CombatUI>();

    }

    public void RegisterService<T>(T service) where T : MonoBehaviour
    {
        var field = GetType().GetField(typeof(T).Name);
        if (field != null)
        {
            field.SetValue(this, service);
        }
    }
}

