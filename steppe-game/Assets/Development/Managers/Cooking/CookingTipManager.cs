using System;
using System.Collections.Generic;
using UnityEngine;

public class CookingTipManager : MonoBehaviour
{
    [Header("Managers")] [SerializeField] private DragDropManager _dragDropManager;
    [SerializeField] private CookingManager _cookingManager;
    [SerializeField] private GuestManager _guestManager;
    [SerializeField] private LevelManager _levelManager;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private SoundManager _soundManager;

    [Header("Plate")] [SerializeField] private GameObject _addSaltPlate;
    [SerializeField] private GameObject _addSugarPlate;
    [SerializeField] private GameObject _addOilPlate;
    [SerializeField] private GameObject _addOilExtendedPlate;
    [SerializeField] private GameObject _addWaterPlate;
    [SerializeField] private GameObject _addWaterDoughPlate;
    [SerializeField] private GameObject _addEggPlate;
    [SerializeField] private GameObject _addFlourPlate;
    [SerializeField] private GameObject _addFlourExtendedPlate;
    [SerializeField] private GameObject _addYestPlate;
    [SerializeField] private GameObject _addMilkPlate;
    [SerializeField] private GameObject _stirPlate;
    [SerializeField] private GameObject _takePlate;
    [SerializeField] private AudioClip _addSaltPlateAudioClip;
    [SerializeField] private AudioClip _addSugarPlateAudioClip;
    [SerializeField] private AudioClip _addOilPlateAudioClip;
    [SerializeField] private AudioClip _addOilExtendedPlateAudioClip;
    [SerializeField] private AudioClip _addWaterPlateAudioClip;
    [SerializeField] private AudioClip _addWaterDoughPlateAudioClip;
    [SerializeField] private AudioClip _addEggPlateAudioClip;
    [SerializeField] private AudioClip _addFlourPlateAudioClip;
    [SerializeField] private AudioClip _addFlourExtendedPlateAudioClip;
    [SerializeField] private AudioClip _addYestPlateAudioClip;
    [SerializeField] private AudioClip _addMilkPlateAudioClip;
    [SerializeField] private AudioClip _stirPlateAudioClip;
    [SerializeField] private AudioClip _takePlateAudioClip;

    [Header("Pot")] [SerializeField] private GameObject _addSaltPot;
    [SerializeField] private GameObject _addMilkPot;
    [SerializeField] private GameObject _addBaursakPot;
    [SerializeField] private GameObject _takeBaursakPot;
    [SerializeField] private GameObject _takeBaursakPot2;
    [SerializeField] private GameObject _takeBoiledKurtPot;
    [SerializeField] private GameObject _takeBoiledKurtPot2;
    [SerializeField] private GameObject _takeKurtPot;
    [SerializeField] private GameObject _takeKurtPot2;
    [SerializeField] private GameObject _burntPot;
    [SerializeField] private GameObject _burntPot2;
    [SerializeField] private GameObject _firePot;
    [SerializeField] private AudioClip _addSaltPotAudioClip;
    [SerializeField] private AudioClip _addMilkPotAudioClip;
    [SerializeField] private AudioClip _takeBaursakPotAudioClip;
    [SerializeField] private AudioClip _takeBoiledKurtPotAudioClip;
    [SerializeField] private AudioClip _takeKurtPotAudioClip;
    [SerializeField] private AudioClip _putInPotAudioClip;

    [Header("Kumis")] [SerializeField] private GameObject _addKumis;
    [SerializeField] private GameObject _takeKumis;
    [SerializeField] private AudioClip _addKumisAudioClip;
    [SerializeField] private AudioClip _takeKumisAudioClip;

    [Header("Cauldrons")] [SerializeField] private GameObject _addOnionCauldron;
    [SerializeField] private GameObject _addMeatCauldron;
    [SerializeField] private GameObject _addSausageCauldron;
    [SerializeField] private GameObject _addDoughCauldron;
    [SerializeField] private GameObject _takeCauldron;
    [SerializeField] private GameObject _takeMeatOut;
    [SerializeField] private GameObject _addOilCauldron;
    [SerializeField] private AudioClip _addOnionCauldronAudioClip;
    [SerializeField] private AudioClip _addMeatCauldronAudioClip;
    [SerializeField] private AudioClip _addSausageCauldronAudioClip;
    [SerializeField] private AudioClip _addDoughCauldronAudioClip;
    [SerializeField] private AudioClip _takeCauldronAudioClip;
    [SerializeField] private AudioClip _takeMeatOutAudioClip;
    [SerializeField] private AudioClip _addOilCauldronAudioClip;

    [Header("Board")] [SerializeField] private GameObject _addOnionBoard;
    [SerializeField] private GameObject _cutOnionBoard;
    [SerializeField] private GameObject _addDoughBoard;
    [SerializeField] private GameObject _cutDoughBoard;
    [SerializeField] private GameObject _rollDoughBoard;
    [SerializeField] private GameObject _cutMeatBoard;
    [SerializeField] private GameObject _takeMeatBoard;
    [SerializeField] private AudioClip _addOnionBoardAudioClip;
    [SerializeField] private AudioClip _cutOnionBoardAudioClip;
    [SerializeField] private AudioClip _addDoughBoardAudioClip;
    [SerializeField] private AudioClip _cutDoughBoardAudioClip;
    [SerializeField] private AudioClip _rollDoughBoardAudioClip;
    [SerializeField] private AudioClip _cutMeatBoardAudioClip;
    [SerializeField] private AudioClip _takeMeatBoardAudioClip;
    [SerializeField] private AudioClip _putBaursakInPotAudioClip;

    [Header("Bowl")] [SerializeField] private GameObject _takeOrderBowl1;
    [SerializeField] private GameObject _takeOrderBowl2;
    [SerializeField] private GameObject _takeBaursakOrderBowl1;
    [SerializeField] private GameObject _takeBaursakOrderBowl2;
    [SerializeField] private GameObject _takeBeshbarmakOrderBowl1;
    [SerializeField] private GameObject _takeBeshbarmakOrderBowl2;
    [SerializeField] private AudioClip _takeOrderBowlAudioClip;
    [SerializeField] private AudioClip _giveBaursakAudioClip;
    [SerializeField] private AudioClip _giveBeshbarmakAudioClip;

    [Header("Other")] [SerializeField] private GameObject _wait;
    [SerializeField] private GameObject _waitMeatCooked;
    [SerializeField] private AudioClip _wAITAudioClip;
    [SerializeField] private AudioClip _waitMeatCookedAudioClip;

    private DateTime _lastActionRealTime;
    private double _hintIntervalSeconds = 6.0;
    private double _initHintIntervalSeconds;
    private bool _isInitialized = false;
    private bool _isFire;

    public void Initialize()
    {
        _lastActionRealTime = DateTime.Now;

        DisableAllHints();
        _isInitialized = true;

        _initHintIntervalSeconds = _hintIntervalSeconds;
    }

    

    private void OnIngredientAdded(IngredientType _, IngredientType __) => OnPlayerAction();

    private void Update()
    {
        if (!_isInitialized)
            return;

        var elapsedSeconds = (DateTime.Now - _lastActionRealTime).TotalSeconds;
        if (elapsedSeconds >= _hintIntervalSeconds)
        {
            ShowHints();
            _lastActionRealTime = DateTime.Now;
            _hintIntervalSeconds = _initHintIntervalSeconds;
        }
    }

    private void SetHintSecsZero() => _hintIntervalSeconds = 0;

    private void OnPlayerAction()
    {
        _lastActionRealTime = DateTime.Now;
        DisableAllHints();
    }

    private void DisableAllHints()
    {
        var allHints = new[]
        {
            _addSaltPlate, _addSugarPlate, _addOilPlate, _addOilExtendedPlate, _addWaterPlate, _addWaterDoughPlate, 
            _addEggPlate, _addFlourPlate, _addFlourExtendedPlate, _addYestPlate, _addMilkPlate, _stirPlate, _takePlate,

            _addSaltPot, _addMilkPot, _addBaursakPot, _takeBaursakPot, _takeBaursakPot2, _takeBoiledKurtPot,
            _takeBoiledKurtPot2, _takeKurtPot, _takeKurtPot2, _burntPot, _burntPot2, _firePot,

            _addKumis, _takeKumis,

            _addOnionCauldron, _addMeatCauldron, _addSausageCauldron, _addDoughCauldron, _takeCauldron, _takeMeatOut,
            _addOilCauldron,

            _addOnionBoard, _cutOnionBoard, _addDoughBoard, _cutDoughBoard, _rollDoughBoard, _cutMeatBoard,
            _takeMeatBoard,

            _takeOrderBowl1, _takeOrderBowl2, _takeBaursakOrderBowl1, _takeBaursakOrderBowl2, _takeBeshbarmakOrderBowl1,
            _takeBeshbarmakOrderBowl2,

            _wait, _waitMeatCooked
        };

        foreach (var h in allHints)
        {
            if (h != null) h.SetActive(false);
        }
    }

    private void PlayHintAudio(AudioClip clip)
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.volume = _soundManager.VolumeVoicesSounds;
        _audioSource.PlayOneShot(clip);
    }

    private List<IngredientType> GetRemainingDishes()
    {
        var remainingDishes = new List<IngredientType>();

        foreach (var guest in _guestManager.GetActiveGuests())
        {
            foreach (var dishStatus in guest.Order.GetUncompletedDishes())
            {
                if (!remainingDishes.Contains(dishStatus.DishData.dishType))
                {
                    remainingDishes.Add(dishStatus.DishData.dishType);
                }
            }
        }

        return remainingDishes;
    }

    private void ShowHints()
    {
        if (_isFire)
        {
            _firePot.SetActive(true);
            PlayHintAudio(_wAITAudioClip);
            return;
        }

        _firePot.SetActive(false);

        DisableAllHints();
        var remainingDishes = GetRemainingDishes();

        if (remainingDishes.Count == 0)
        {
            return;
        }

        foreach (var dish in remainingDishes)
        {
            if (dish == IngredientType.Kumis)
            {
                if (!_cookingManager.HasIngredient(IngredientType.KumisCup, IngredientType.Kumis))
                {
                    _addKumis.SetActive(true);
                    PlayHintAudio(_addKumisAudioClip);
                    return;
                }
                else
                {
                    _takeKumis.SetActive(true);
                    PlayHintAudio(_takeKumisAudioClip);
                    return;
                }
            }
            else if (dish == IngredientType.BaursakCooked)
            {
                if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.BaursakBurned))
                {
                    _burntPot.SetActive(true);
                    PlayHintAudio(_takePlateAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.BaursakBurned))
                {
                    _burntPot2.SetActive(true);
                    PlayHintAudio(_takePlateAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.Baursak) ||
                    _cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.Baursak))
                {
                    _wait.SetActive(true);
                    PlayHintAudio(_wAITAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.BaursakCooked))
                {
                    _takeBaursakPot.SetActive(true);
                    PlayHintAudio(_takeBaursakPotAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.BaursakCooked))
                {
                    _takeBaursakPot2.SetActive(true);
                    PlayHintAudio(_takeBaursakPotAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Bowl1, IngredientType.BaursakCooked))
                {
                    _takeOrderBowl1.SetActive(true);
                    PlayHintAudio(_takeOrderBowlAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Bowl2, IngredientType.BaursakCooked))
                {
                    _takeOrderBowl2.SetActive(true);
                    PlayHintAudio(_takeOrderBowlAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.BaursakBurned))
                {
                    _burntPot.SetActive(true);
                    PlayHintAudio(_takePlateAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.BaursakBurned))
                {
                    _burntPot2.SetActive(true);
                    PlayHintAudio(_takePlateAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Board, IngredientType.DoughBaursak))
                {
                    //_rollDoughBoard.SetActive(true);
                    //PlayHintAudio(_rollDoughBoardAudioClip);
                    _cutDoughBoard.SetActive(true);
                    PlayHintAudio(_cutDoughBoardAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Board, IngredientType.Baursak) &&
                    !_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.Oil) &&
                    !_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.Oil))
                {
                    _addOilCauldron.SetActive(true);
                    PlayHintAudio(_addOilCauldronAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.Oil))
                {
                    _addBaursakPot.SetActive(true);
                    //PlayHintAudio(_putInPotAudioClip);
                    PlayHintAudio(_putBaursakInPotAudioClip);
                    return;
                }

                var hasFlour = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Flour);
                var hasEgg   = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Egg);
                var hasOil   = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Oil);
                var hasSugar = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Sugar);
                var hasSalt  = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Salt);
                var hasMilk  = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Milk);
                var hasYest  = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Yest);
                var hasWater = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Water);
                
                if (hasFlour && hasEgg && hasOil && hasSugar && hasMilk && hasYest && hasWater && hasSalt)
                {
                    _stirPlate.SetActive(true);
                    PlayHintAudio(_stirPlateAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.DoughBaursak))
                {
                    _addDoughBoard.SetActive(true);
                    PlayHintAudio(_addDoughBoardAudioClip);
                    return;
                }

                if (!hasFlour)
                {
                    _addFlourPlate.SetActive(true);
                    PlayHintAudio(_addFlourPlateAudioClip);
                    return;
                }
                else if (!hasEgg)
                {
                    _addEggPlate.SetActive(true);
                    PlayHintAudio(_addEggPlateAudioClip);
                    return;
                }
                else if (!hasOil)
                {
                    _addOilPlate.SetActive(true);
                    PlayHintAudio(_addOilPlateAudioClip);
                    return;
                }
                else if (!hasSugar)
                {
                    _addSugarPlate.SetActive(true);
                    PlayHintAudio(_addSugarPlateAudioClip);
                    return;
                }
                else if (!hasSalt)
                {
                    _addSaltPlate.SetActive(true);
                    PlayHintAudio(_addSaltPlateAudioClip);
                    return;
                }
                else if (!hasMilk)
                {
                    _addMilkPlate.SetActive(true);
                    PlayHintAudio(_addMilkPlateAudioClip);
                    return;
                }
                else if (!hasYest)
                {
                    _addYestPlate.SetActive(true);
                    PlayHintAudio(_addYestPlateAudioClip);
                    return;
                }
                else if (!hasWater)
                {
                    _addWaterPlate.SetActive(true);
                    PlayHintAudio(_addWaterPlateAudioClip);
                    return;
                }
            }
            else if (dish == IngredientType.KurutCooked)
            {
                if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.MilkCooked))
                {
                    _takeBoiledKurtPot.SetActive(true);
                    PlayHintAudio(_takeBoiledKurtPotAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.MilkCooked))
                {
                    _takeBoiledKurtPot2.SetActive(true);
                    PlayHintAudio(_takeBoiledKurtPotAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Pot, IngredientType.KurutCooked))
                {
                    _takeKurtPot.SetActive(true);
                    PlayHintAudio(_takeKurtPotAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.KurutCooked))
                {
                    _takeKurtPot2.SetActive(true);
                    PlayHintAudio(_takeKurtPotAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.Bowl1, IngredientType.KurutCooked))
                {
                    _takeOrderBowl1.SetActive(true);
                    PlayHintAudio(_takeOrderBowlAudioClip);
                    return;
                }
                else if (_cookingManager.HasIngredient(IngredientType.Bowl2, IngredientType.KurutCooked))
                {
                    _takeOrderBowl2.SetActive(true);
                    PlayHintAudio(_takeOrderBowlAudioClip);
                    return;
                }

                var hasMilkPot = _cookingManager.HasIngredient(IngredientType.Pot, IngredientType.Milk)
                                 || _cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.Milk);
                var hasSaltPot = _cookingManager.HasIngredient(IngredientType.Pot, IngredientType.Salt)
                                 || _cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.Salt);
                var hasKurutCooked = _cookingManager.HasIngredient(IngredientType.Pot, IngredientType.KurutCooked)
                                 || _cookingManager.HasIngredient(IngredientType.Pot2, IngredientType.KurutCooked);

                if (hasMilkPot && hasSaltPot)
                {
                    _wait.SetActive(true);
                    PlayHintAudio(_wAITAudioClip);
                    return;
                }

                if (!hasMilkPot && !hasKurutCooked)
                {
                    _addMilkPot.SetActive(true);
                    PlayHintAudio(_addMilkPotAudioClip);
                    return;
                }
                else if (hasMilkPot && !hasSaltPot)
                {
                    _addSaltPot.SetActive(true);
                    PlayHintAudio(_addSaltPotAudioClip);
                    return;
                }
            }
            else if (dish == IngredientType.BershmakCooked)
            {
                var hasOnion = _cookingManager.HasIngredient(IngredientType.Board, IngredientType.Onion);
                var hasOnionPeeled = _cookingManager.HasIngredient(IngredientType.Board, IngredientType.OnionPeeled);
                var hasMeat = _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.Meat);
                var hasSausage = _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.Sausage);
                var hasMeatCutted =
                    _cookingManager.HasIngredient(IngredientType.Board, IngredientType.BershmakMeatCutted);
                var hasMeatCooked =
                    _cookingManager.HasIngredient(IngredientType.Board, IngredientType.MeatCooked);
                var hasDough = _cookingManager.HasIngredient(IngredientType.Board, IngredientType.DoughBershmak);
                var hasDoughCutted =
                    _cookingManager.HasIngredient(IngredientType.Board, IngredientType.DoughBershmakCutted);
                var hasOnionInCauldron =
                    _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.OnionPeeled);
                var hasDoughInCauldron =
                    _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.DoughBershmakCutted);
                var isBoiled = _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.BershmakBoiled);
                var isCooked = _cookingManager.HasIngredient(IngredientType.Cauldron, IngredientType.BershmakCooked);
                var isCookedBowl1 = _cookingManager.HasIngredient(IngredientType.Bowl1, IngredientType.BershmakCooked);
                var isCookedBowl2 = _cookingManager.HasIngredient(IngredientType.Bowl2, IngredientType.BershmakCooked);

                if (hasMeatCooked)
                {
                    _cutMeatBoard.SetActive(true);
                    PlayHintAudio(_cutMeatBoardAudioClip);
                    return;
                }

                if (hasMeatCutted)
                {
                    _takeMeatBoard.SetActive(true);
                    PlayHintAudio(_takeMeatBoardAudioClip);
                    return;
                }

                if (isCookedBowl1)
                {
                    _takeBeshbarmakOrderBowl1.SetActive(true);
                    PlayHintAudio(_giveBeshbarmakAudioClip);
                    return;
                }

                if (isCookedBowl2)
                {
                    _takeBeshbarmakOrderBowl2.SetActive(true);
                    PlayHintAudio(_giveBeshbarmakAudioClip);
                    return;
                }

                if (hasSausage && hasMeat && hasOnionInCauldron && hasDoughInCauldron)
                {
                    _waitMeatCooked.SetActive(true);
                    PlayHintAudio(_waitMeatCookedAudioClip);
                    return;
                }

                if (isBoiled)
                {
                    _takeMeatOut.SetActive(true);
                    PlayHintAudio(_takeMeatOutAudioClip);
                    return;
                }

                if (isCooked)
                {
                    _takeCauldron.SetActive(true);
                    PlayHintAudio(_takeCauldronAudioClip);
                    return;
                }

                if (!hasMeat)
                {
                    _addMeatCauldron.SetActive(true);
                    PlayHintAudio(_addMeatCauldronAudioClip);
                    return;
                }

                if (!hasSausage)
                {
                    _addSausageCauldron.SetActive(true);
                    PlayHintAudio(_addSausageCauldronAudioClip);
                    return;
                }

                if (hasOnionPeeled && !hasOnionInCauldron)
                {
                    _addOnionCauldron.SetActive(true);
                    PlayHintAudio(_addOnionCauldronAudioClip);
                    return;
                }

                else if (hasOnion && !hasOnionPeeled && !hasOnionInCauldron)
                {
                    _cutOnionBoard.SetActive(true);
                    PlayHintAudio(_cutOnionBoardAudioClip);
                    return;
                }

                if (!hasOnion && !hasOnionPeeled && !hasOnionInCauldron)
                {
                    _addOnionBoard.SetActive(true);
                    PlayHintAudio(_addOnionBoardAudioClip);
                    return;
                }

                if (hasDoughCutted && !hasDoughInCauldron)
                {
                    _addDoughCauldron.SetActive(true);
                    PlayHintAudio(_addDoughCauldronAudioClip);
                    return;
                }

                if (hasMeatCutted)
                {
                    _cutMeatBoard.SetActive(true);
                    PlayHintAudio(_cutMeatBoardAudioClip);
                    return;
                }

                if (!hasDough && _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.DoughBershmak))
                {
                    _addDoughBoard.SetActive(true);
                    PlayHintAudio(_addDoughBoardAudioClip);
                    return;
                }
                else if (hasDough && !hasDoughCutted)
                {
                    _cutDoughBoard.SetActive(true);
                    PlayHintAudio(_cutDoughBoardAudioClip);
                    return;
                }

                var hasFlour = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Flour);
                var hasWater = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Water);
                var hasOil = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Oil);
                var hasEgg = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Egg);
                var hasSalt = _cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.Salt);

                if (hasFlour && hasEgg && hasOil && hasWater && hasSalt)
                {
                    _stirPlate.SetActive(true);
                    PlayHintAudio(_stirPlateAudioClip);
                    return;
                }

                if (_cookingManager.HasIngredient(IngredientType.DoughBowl, IngredientType.DoughBaursak))
                {
                    _addDoughBoard.SetActive(true);
                    PlayHintAudio(_addDoughBoardAudioClip);
                    return;
                }

                if (!hasFlour)
                {
                    _addFlourExtendedPlate.SetActive(true);
                    PlayHintAudio(_addFlourExtendedPlateAudioClip);
                    return;
                }
                else if (!hasWater)
                {
                    _addWaterDoughPlate.SetActive(true);
                    PlayHintAudio(_addWaterDoughPlateAudioClip);
                    return;
                }
                else if (!hasOil)
                {
                    _addOilExtendedPlate.SetActive(true);
                    PlayHintAudio(_addOilExtendedPlateAudioClip);
                    return;
                }
                else if (!hasEgg)
                {
                    _addEggPlate.SetActive(true);
                    PlayHintAudio(_addEggPlateAudioClip);
                    return;
                }
                else if (!hasSalt)
                {
                    _addSaltPlate.SetActive(true);
                    PlayHintAudio(_addSaltPlateAudioClip);
                    return;
                }
            }
        }
    }


    public void EndFire()
    {
        _isFire = false;
        ShowHints();
    }

    public void StartFire()
    {
        _isFire = true;
        ShowHints();
    }
    
    private void ResetSettings()
    {
        _hintIntervalSeconds = _initHintIntervalSeconds;
    }

    private void OnEnable()
    {
        _dragDropManager.OnIngredientAdded += OnIngredientAdded;
        _cookingManager.OnDoughPlateClickedAction += OnPlayerAction;
        _cookingManager.OnOnionClickedAction += OnPlayerAction;
        //CookingManager.OnKumisBottleClickedAction += OnPlayerAction;
        _cookingManager.OnCauldronClickedAction += OnPlayerAction;
        _cookingManager.OnPotClickedAction += OnPlayerAction;
        GuestManager.OnComplete += OnPlayerAction;
        LevelManager.OnBackClick += OnPlayerAction;
        LevelManager.OnNextLevelClickedAction += OnPlayerAction;

        CookingManager.OnPotTimerStarted += SetHintSecsZero;
    }
    
    private void OnDisable()
    {
        _dragDropManager.OnIngredientAdded -= OnIngredientAdded;
        _cookingManager.OnDoughPlateClickedAction -= OnPlayerAction;
        _cookingManager.OnOnionClickedAction -= OnPlayerAction;
        //CookingManager.OnKumisBottleClickedAction -= OnPlayerAction;
        _cookingManager.OnCauldronClickedAction -= OnPlayerAction;
        _cookingManager.OnPotClickedAction -= OnPlayerAction;
        GuestManager.OnComplete -= OnPlayerAction;
        LevelManager.OnBackClick -= OnPlayerAction;
        LevelManager.OnNextLevelClickedAction -= OnPlayerAction;

        CookingManager.OnPotTimerStarted -= SetHintSecsZero;
        
        ResetSettings();
    }
}