using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{

    [Header("Mapas combinados")]
    public RawImage minimapaInfluencia;
    public RawImage minimapaTension;

    [Header("Toggles")]
    public Toggle toggleInfluencia;
    public Toggle toggleTension;

    private void Start()
    {
        // Inicializa el estado visual según el valor inicial del toggle
        ActualizarVisibilidad(minimapaInfluencia, toggleInfluencia);
        ActualizarVisibilidad(minimapaTension, toggleTension);

        // Suscribe cada toggle a su mapa
        toggleInfluencia.onValueChanged.AddListener(valor => minimapaInfluencia.gameObject.SetActive(valor));
        toggleTension.onValueChanged.AddListener(valor => minimapaTension.gameObject.SetActive(valor));
    }

    private void ActualizarVisibilidad(RawImage mapa, Toggle toggle)
    {
        if (mapa == null || toggle == null) return;
        mapa.gameObject.SetActive(toggle.isOn);
    }
}