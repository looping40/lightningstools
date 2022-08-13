using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using F4SharedMem;
using F4SharedMem.Headers;
using F4Utils.SimSupport;

public class Adi : MonoBehaviour
{

    //debug
    [SerializeField] bool useDebug = false;
    //Ball
    [SerializeField] float pitchAngle;
    [SerializeField] float rollAngle;

    //Slip
    [SerializeField] float slip;

    //turn rate
    [SerializeField] float turnRate;

    //Ils
    [SerializeField] float glideAngle;
    [SerializeField] float locAngle;
    
    //Flags
    [SerializeField] bool showGsFlag;
    [SerializeField] bool showLocFlag;
    [SerializeField] bool showOffFlag;
    [SerializeField] bool showAuxFlag;

    //Ball
    private GameObject _BallGameObject;

    //Bank
    private GameObject _bankGameObject;

    //Slip
    private GameObject _SlipGameObject;

    //turn rate
    private GameObject _TurnRateGameObject;

    //Ils
    private GameObject _GsGameObject;
    private GameObject _LocGameObject;

    //Flags
    private GameObject _FlagGsGameObject;
    private GameObject _FlagLocGameObject;
    private GameObject _FlagOffGameObject;
    private GameObject _FlagAuxGameObject;

    private readonly Reader _sharedMemReader = new();
    private FlightData _lastFlightData;

    private readonly IIndicatedRateOfTurnCalculator _rateOfTurnCalculator = new IndicatedRateOfTurnCalculator();

    private const float GLIDESLOPE_SCALE = 5.0f;
    private const float LOCALIZER_SCALE = 1.0f;
    private const float RADIANS_PER_DEGREE = 0.0174532925f;
    private const float DEGREES_PER_RADIAN = 57.2957795f;

    private FlightData ReadSharedMem()
    {
        return _lastFlightData = _sharedMemReader.GetCurrentData();
    }

    private float DegToRadians(float angle)
    {
        return angle / RADIANS_PER_DEGREE;
    }

    // Start is called before the first frame update
    void Start()
    {
        _BallGameObject = GameObject.Find("Ball");
        _bankGameObject = GameObject.Find("Bank");
        _SlipGameObject = GameObject.Find("Slip");
        _TurnRateGameObject  = GameObject.Find("TurnRate");
        _GsGameObject = GameObject.Find("Gs");
        _LocGameObject = GameObject.Find("Loc");
        _FlagGsGameObject = GameObject.Find("FlagGs");
        _FlagLocGameObject = GameObject.Find("FlagLoc");
        _FlagOffGameObject = GameObject.Find("FlagOff");
        _FlagAuxGameObject = GameObject.Find("FlagAux");
}

    // Update is called once per frame
    void Update()
    {
        if (useDebug)
        {
            _BallGameObject.transform.rotation = Quaternion.Euler(new Vector3(rollAngle, -90, pitchAngle));
            _bankGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rollAngle));

            _SlipGameObject.transform.position = new Vector3(slip, _SlipGameObject.transform.position.y, _SlipGameObject.transform.position.z);
            _TurnRateGameObject.transform.position = new Vector3(turnRate * 2, _TurnRateGameObject.transform.position.y, _TurnRateGameObject.transform.position.z);


            _GsGameObject.transform.rotation = Quaternion.Euler(new Vector3(glideAngle, 0, 0));
            _LocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, locAngle, 0));

            if (showGsFlag)
            {
                _FlagGsGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -30));
            }
            else
            {
                _FlagGsGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }

            if (showLocFlag)
            {
                _FlagLocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 30));
            }
            else
            {
                _FlagLocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }

            if (showOffFlag)
            {
                _FlagOffGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 30));
            }
            else
            {
                _FlagOffGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }

            if (showOffFlag || showAuxFlag)
            {
                _FlagAuxGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -30));
            }
            else
            {
                _FlagAuxGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            }
        }
        else
        {
            if (ReadSharedMem() != null)
            {
                _BallGameObject.transform.rotation = Quaternion.Euler(new Vector3(-DegToRadians(_lastFlightData.roll), -90, DegToRadians(-_lastFlightData.pitch)));
                _bankGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -DegToRadians(_lastFlightData.roll)));

                _SlipGameObject.transform.position = new Vector3(_lastFlightData.beta, _SlipGameObject.transform.position.y, _SlipGameObject.transform.position.z);

                _GsGameObject.transform.rotation = Quaternion.Euler(new Vector3(GLIDESLOPE_SCALE * DegToRadians(-_lastFlightData.AdiIlsVerPos), 0, 0));
                _LocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, LOCALIZER_SCALE * DegToRadians(-_lastFlightData.AdiIlsHorPos), 0));

                var rateOfTurn = _rateOfTurnCalculator.DetermineIndicatedRateOfTurn(_lastFlightData.yaw * DEGREES_PER_RADIAN);
                _TurnRateGameObject.transform.position = new Vector3(rateOfTurn * 2, _TurnRateGameObject.transform.position.y, _TurnRateGameObject.transform.position.z);

                var hsiBits = (HsiBits)_lastFlightData.hsiBits;
                bool _showAuxFlag = ((hsiBits & HsiBits.ADI_AUX) == HsiBits.ADI_AUX) || ((hsiBits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF);
                bool _showOffFlag = (hsiBits & HsiBits.ADI_OFF) == HsiBits.ADI_OFF;
                bool _showGsFlag = (hsiBits & HsiBits.ADI_GS) == HsiBits.ADI_GS;
                bool _showLocFlag = (hsiBits & HsiBits.ADI_LOC) == HsiBits.ADI_LOC;

                if (_showGsFlag)
                {
                    _FlagGsGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -30));
                }
                else
                {
                    _FlagGsGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }

                if (_showLocFlag)
                {
                    _FlagLocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 30));
                }
                else
                {
                    _FlagLocGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }

                if (_showOffFlag)
                {
                    _FlagOffGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 30));
                }
                else
                {
                    _FlagOffGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }

                if (_showAuxFlag)
                {
                    _FlagAuxGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -30));
                }
                else
                {
                    _FlagAuxGameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }
            }
        }

    }
}
