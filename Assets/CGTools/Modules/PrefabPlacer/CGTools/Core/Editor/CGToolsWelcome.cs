using UnityEditor;
using UnityEngine;

namespace CGTools.Core
{
    /// <summary>
    /// Welcome window shown on first launch of CGTools.
    /// </summary>
    public class CGToolsWelcome : EditorWindow
    {
        private Vector2 scrollPosition;
        private Texture2D bannerTexture;
        private CGToolsSettings settings;
        private SystemLanguage selectedLanguage;

        private int currentPage = 0;
        private const int TOTAL_PAGES = 3;

        // Cached styles
        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle wordWrappedStyle;

        private GUIStyle TitleStyle => titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        private GUIStyle HeaderStyle => headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14
        };

        private GUIStyle WordWrappedStyle => wordWrappedStyle ??= new GUIStyle(EditorStyles.label)
        {
            wordWrap = true
        };

        #region Window Management

        public static void ShowWindow()
        {
            CGToolsWelcome window = GetWindow<CGToolsWelcome>(true, "Welcome to CGTools", true);
            window.minSize = new Vector2(600, 500);
            window.maxSize = new Vector2(600, 500);
            window.Show();
        }

        private void OnEnable()
        {
            settings = CGToolsSettings.Instance;
            selectedLanguage = settings.Language;
            LoadBanner();
        }

        private void OnGUI()
        {
            switch (currentPage)
            {
                case 0: DrawWelcomePage(); break;
                case 1: DrawFeaturesPage(); break;
                case 2: DrawSetupPage(); break;
            }

            DrawNavigationButtons();
        }

        #endregion

        #region Page 1: Welcome

        private void DrawWelcomePage()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(20);

            if (bannerTexture != null)
            {
                float aspectRatio = (float)bannerTexture.height / bannerTexture.width;
                Rect bannerRect = GUILayoutUtility.GetRect(position.width - 40, (position.width - 40) * aspectRatio);
                GUI.DrawTexture(bannerRect, bannerTexture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUILayout.Label("🎮 CGTOOLS", TitleStyle);
            }

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Welcome to CGTools!", "¡Bienvenido a CGTools!"), TitleStyle);

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(GetLocalizedText(
                "Thank you for installing CGTools - a professional suite of Unity editor extensions designed to boost your productivity and streamline your workflow.",
                "Gracias por instalar CGTools - una suite profesional de extensiones de Unity Editor diseñadas para aumentar tu productividad y optimizar tu flujo de trabajo."
            ), MessageType.Info);

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("What is CGTools?", "¿Qué es CGTools?"), HeaderStyle);
            GUILayout.Space(5);

            EditorGUILayout.LabelField(GetLocalizedText(
                "CGTools is a modular system of editor extensions that helps Unity developers work faster and smarter. Each module is designed to solve specific workflow challenges while maintaining zero runtime overhead.",
                "CGTools es un sistema modular de extensiones de editor que ayuda a los desarrolladores de Unity a trabajar más rápido e inteligente. Cada módulo está diseñado para resolver desafíos específicos del flujo de trabajo sin impacto en el rendimiento del juego."
            ), WordWrappedStyle);

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Core Principles:", "Principios Fundamentales:"), HeaderStyle);

            DrawBulletPoint("✅", GetLocalizedText("100% Free & Fully Functional", "100% Gratuito y Completamente Funcional"));
            DrawBulletPoint("⚡", GetLocalizedText("Zero Runtime Overhead (Editor-Only)", "Cero Impacto en Runtime (Solo Editor)"));
            DrawBulletPoint("🧩", GetLocalizedText("Modular Architecture", "Arquitectura Modular"));
            DrawBulletPoint("🌍", GetLocalizedText("Bilingual Support (English/Spanish)", "Soporte Bilingüe (Inglés/Español)"));
            DrawBulletPoint("❤️", GetLocalizedText("Community-Driven Development", "Desarrollo Impulsado por la Comunidad"));

            GUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Page 2: Features

        private void DrawFeaturesPage()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("What's Included", "¿Qué Incluye?"), TitleStyle);

            GUILayout.Space(15);

            DrawFeatureSection("🎮",
                GetLocalizedText("CGTools Core", "CGTools Core"),
                GetLocalizedText(
                    "Central hub for managing all modules, settings, and preferences. Access everything from one convenient location.",
                    "Hub central para gestionar todos los módulos, configuración y preferencias. Accede a todo desde una ubicación conveniente."
                )
            );

            DrawFeatureSection("🔍",
                GetLocalizedText("Auto Module Detection", "Detección Automática de Módulos"),
                GetLocalizedText(
                    "Automatically detects and registers installed modules. No manual setup required.",
                    "Detecta y registra automáticamente los módulos instalados. No requiere configuración manual."
                )
            );

            DrawFeatureSection("⚙️",
                GetLocalizedText("Persistent Settings", "Configuración Persistente"),
                GetLocalizedText(
                    "Your preferences are saved and restored automatically across Unity sessions.",
                    "Tus preferencias se guardan y restauran automáticamente entre sesiones de Unity."
                )
            );

            DrawFeatureSection("🌍",
                GetLocalizedText("Bilingual Interface", "Interfaz Bilingüe"),
                GetLocalizedText(
                    "Switch between English and Spanish seamlessly. All modules support both languages.",
                    "Cambia entre inglés y español sin problemas. Todos los módulos soportan ambos idiomas."
                )
            );

            DrawFeatureSection("📦",
                GetLocalizedText("Expandable Module System", "Sistema de Módulos Expandible"),
                GetLocalizedText(
                    "Start with the modules you need, add more later. Each module is independent and can be installed separately.",
                    "Comienza con los módulos que necesitas, agrega más después. Cada módulo es independiente y puede instalarse por separado."
                )
            );

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Currently Installed Modules:", "Módulos Actualmente Instalados:"), HeaderStyle);
            GUILayout.Space(5);

            var stats = ModuleManager.GetStatistics();
            if (stats.AvailableModules > 0)
            {
                foreach (var module in ModuleManager.AvailableModules)
                {
                    DrawBulletPoint("📦", $"{module.GetLocalizedName(selectedLanguage)} - {module.GetLocalizedDescription(selectedLanguage)}");
                }
            }
            else
            {
                EditorGUILayout.HelpBox(GetLocalizedText(
                    "No modules detected. Install a module package to get started.",
                    "No se detectaron módulos. Instala un paquete de módulo para comenzar."
                ), MessageType.Warning);
            }

            GUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Page 3: Setup

        private void DrawSetupPage()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Quick Setup", "Configuración Rápida"), TitleStyle);

            GUILayout.Space(15);

            GUILayout.Label(GetLocalizedText("Preferred Language", "Idioma Preferido"), HeaderStyle);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            selectedLanguage = (SystemLanguage)EditorGUILayout.EnumPopup(selectedLanguage);
            if (EditorGUI.EndChangeCheck())
                settings.Language = selectedLanguage;

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Show Hub on Unity Startup", "Mostrar Hub al Iniciar Unity"), HeaderStyle);
            GUILayout.Space(5);

            bool newShowOnStartup = EditorGUILayout.Toggle(
                GetLocalizedText("Open automatically", "Abrir automáticamente"),
                settings.ShowHubOnStartup
            );
            if (newShowOnStartup != settings.ShowHubOnStartup)
                settings.ShowHubOnStartup = newShowOnStartup;

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Quick Start Guide", "Guía de Inicio Rápido"), HeaderStyle);
            GUILayout.Space(10);

            DrawNumberedStep("1", GetLocalizedText(
                "Access CGTools Hub from Tools > CGTools > Hub",
                "Accede al Hub de CGTools desde Tools > CGTools > Hub"
            ));
            DrawNumberedStep("2", GetLocalizedText(
                "Browse installed modules in the main window",
                "Explora los módulos instalados en la ventana principal"
            ));
            DrawNumberedStep("3", GetLocalizedText(
                "Click 'Open' on any module to start using it",
                "Haz clic en 'Abrir' en cualquier módulo para comenzar a usarlo"
            ));
            DrawNumberedStep("4", GetLocalizedText(
                "Check documentation for detailed module guides",
                "Consulta la documentación para guías detalladas de cada módulo"
            ));

            GUILayout.Space(20);

            GUILayout.Label(GetLocalizedText("Need Help?", "¿Necesitas Ayuda?"), HeaderStyle);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(GetLocalizedText("📖 Documentation", "📖 Documentación")))
                Application.OpenURL("https://github.com/cubasgames/cgtools");

            if (GUILayout.Button(GetLocalizedText("📧 Contact", "📧 Contacto")))
                Application.OpenURL("mailto:support@cubasgames.com");

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Navigation

        private void DrawNavigationButtons()
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{currentPage + 1} / {TOTAL_PAGES}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(currentPage == 0);
            if (GUILayout.Button(GetLocalizedText("← Previous", "← Anterior"), GUILayout.Height(30)))
            {
                currentPage--;
                scrollPosition = Vector2.zero;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            if (currentPage < TOTAL_PAGES - 1)
            {
                if (GUILayout.Button(GetLocalizedText("Next →", "Siguiente →"), GUILayout.Height(30)))
                {
                    currentPage++;
                    scrollPosition = Vector2.zero;
                }
            }
            else
            {
                if (GUILayout.Button(GetLocalizedText("✅ Get Started!", "✅ ¡Comenzar!"), GUILayout.Height(30), GUILayout.Width(150)))
                    FinishWelcome();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (currentPage < TOTAL_PAGES - 1)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(GetLocalizedText("Skip", "Omitir"), EditorStyles.miniButton))
                    FinishWelcome();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        private void FinishWelcome()
        {
            settings.ShowWelcomeScreen = false;
            Close();
            EditorApplication.delayCall += CGToolsHub.ShowWindow;
        }

        #endregion

        #region UI Helpers

        private void LoadBanner()
        {
            string[] possiblePaths =
            {
                "Assets/CGTools/Core/Resources/Images/Banner.png",
                "Assets/CGTools/Core/Resources/Images/Logo.png",
                "Assets/CGTools/Core/Resources/Banner.png"
            };

            foreach (string path in possiblePaths)
            {
                bannerTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (bannerTexture != null)
                    break;
            }
        }

        private void DrawFeatureSection(string icon, string title, string description)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(icon, GUILayout.Width(30), GUILayout.Height(30));
            EditorGUILayout.BeginVertical();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Label(description, WordWrappedStyle);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void DrawBulletPoint(string icon, string text)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(icon, GUILayout.Width(20));
            GUILayout.Label(text, WordWrappedStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private void DrawNumberedStep(string number, string text)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(number, EditorStyles.boldLabel, GUILayout.Width(20));
            GUILayout.Label(text, WordWrappedStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private string GetLocalizedText(string english, string spanish)
        {
            return selectedLanguage == SystemLanguage.Spanish ? spanish : english;
        }

        #endregion
    }
}