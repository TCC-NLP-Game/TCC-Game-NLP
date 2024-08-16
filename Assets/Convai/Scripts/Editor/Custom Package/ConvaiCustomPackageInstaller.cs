#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Convai.Scripts.Editor.Custom_Package
{
    /// <summary>
    ///     Custom package installer for Convai's Custom Packages in Unity Editor.
    /// </summary>
    public class ConvaiCustomPackageInstaller : EditorWindow, IActiveBuildTargetChanged
    {
        // Enum to represent different setup types
        private enum SetupTypes
        {
            None,
            ARAndroid,
            ARiOS,
            VR,
            Uninstaller
        }

        // Paths to different Convai packages
        private const string AR_PACKAGE_PATH = "Assets/Convai/Custom Packages/ConvaiARUpgrader.unitypackage";
        private const string IOS_BUILD_PACKAGE_PATH = "Assets/Convai/Custom Packages/ConvaiiOSBuild.unitypackage";
        private const string TMP_PACKAGE_PATH = "Assets/Convai/Custom Packages/ConvaiCustomTMP.unitypackage";
        private const string URP_CONVERTER_PACKAGE_PATH = "Assets/Convai/Custom Packages/ConvaiURPConverter.unitypackage";
        private const string VR_PACKAGE_PATH = "Assets/Convai/Custom Packages/ConvaiVRUpgrader.unitypackage";

        // Index to keep track of the current package installation step
        private int _currentPackageInstallIndex;

        // Current setup type
        private SetupTypes _currentSetup;

        // Request object for package installations/uninstallations
        private Request _request;

        /// <summary>
        ///     GUI method to display the window and buttons.
        /// </summary>
        private void OnGUI()
        {
            // Loading Convai logo
            Texture2D convaiLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(ConvaiImagesDirectory.CONVAI_LOGO_PATH);
            GUI.DrawTexture(new Rect(115, 0, 256, 80), convaiLogo);

            GUILayout.BeginArea(new Rect(165, 100, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            // Button to install AR package
            if (GUILayout.Button("Install AR Package", GUILayout.Width(170), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Which Platform",
                        "Which platform do you want to install AR package for?", "Android", "iOS"))
                {
                    // Display confirmation dialog before installation
                    if (EditorUtility.DisplayDialog("Confirm Android AR Package Installation",
                            "This step will install AR-related packages and integrate Convai's AR package into your project. " +
                            "This process will affect your project. Do you want to proceed?\n\n" +
                            "The following operations will be performed:\n" +
                            "- Universal Render Pipeline (URP)\n" +
                            "- ARCore Plugin\n" +
                            "- Convai Custom AR Package\n" +
                            "- Convai URP Converter\n\n" +
                            "* If these packages are not present in your project, they will be installed.\n" +
                            "* If the target build platform is not Android, it will be switched to Android.", "Yes, Proceed", "No, Cancel"))
                    {
                        EditorApplication.LockReloadAssemblies();
                        StartPackageInstallation(SetupTypes.ARAndroid);
                    }
                }
                else
                {
                    // Display confirmation dialog before installation
                    if (EditorUtility.DisplayDialog("Confirm iOS AR Package Installation",
                            "This step will install AR-related packages and integrate Convai's AR package into your project. " +
                            "This process will affect your project. Do you want to proceed?\n\n" +
                            "The following operations will be performed:\n" +
                            "- Universal Render Pipeline (URP)\n" +
                            "- ARKit Plugin\n" +
                            "- Convai Custom AR Package\n" +
                            "- Convai URP Converter\n\n" +
                            "* If these packages are not present in your project, they will be installed.\n" +
                            "* If the target build platform is not iOS, it will be switched to iOS.", "Yes, Proceed", "No, Cancel"))
                    {
                        EditorApplication.LockReloadAssemblies();
                        StartPackageInstallation(SetupTypes.ARiOS);
                    }
                }
            }

            GUILayout.Space(10);

            // Button to install VR package
            if (GUILayout.Button("Install VR Package", GUILayout.Width(170), GUILayout.Height(30)))
                // Display confirmation dialog before installation
                if (EditorUtility.DisplayDialog("Confirm VR Package Installation",
                        "This step will install VR-related packages and integrate Convai's VR package into your project. " +
                        "This process will affect your project. Do you want to proceed?\n\n" +
                        "The following operations will be performed:\n" +
                        "- Universal Render Pipeline (URP)\n" +
                        "- OpenXR Plugin\n" +
                        "- XR Interaction Toolkit\n" +
                        "- Convai Custom VR Package\n" +
                        "- Convai URP Converter\n\n" +
                        "* If these packages are not present in your project, they will be installed.\n" +
                        "* If the target build platform is not Android, it will be switched to Android.", "Yes, Proceed", "No, Cancel"))
                {
                    EditorApplication.LockReloadAssemblies();
                    StartPackageInstallation(SetupTypes.VR);
                }

            GUILayout.Space(10);

            // Button to uninstall XR package
            if (GUILayout.Button("Uninstall XR Package", GUILayout.Width(170), GUILayout.Height(30)))
                // Display confirmation dialog before uninstallation
                if (EditorUtility.DisplayDialog("Confirm Package Uninstallation",
                        "This process will uninstall the Convai package and revert changes made by AR or VR setups in your project. " +
                        "It may affect your project. Are you sure you want to proceed?\n\n" +
                        "The following packages will be uninstalled.\n" +
                        "- ARCore Plugin or ARKit\n" +
                        "- OpenXR Plugin\n" +
                        "- XR Interaction Toolkit\n" +
                        "- Convai Custom AR or VR Package\n\n" +
                        "* The Convai Uninstaller Package will be installed. This process will revert scripts modified for XR to their default states.",
                        "Yes, Uninstall", "No, Cancel"))
                {
                    _currentSetup = SetupTypes.Uninstaller;
                    EditorApplication.update += Progress;
                    EditorApplication.LockReloadAssemblies();
                    HandleUninstallPackage();
                }

            GUILayout.Space(10);

            if (GUILayout.Button("Install iOS Build Package", GUILayout.Width(170), GUILayout.Height(30)))
            {
                InstallConvaiUnityPackage(IOS_BUILD_PACKAGE_PATH);
                TryToDownloadiOSDLL();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Install URP Converter", GUILayout.Width(170), GUILayout.Height(30))) InstallConvaiUnityPackage(URP_CONVERTER_PACKAGE_PATH);

            GUILayout.Space(10);

            if (GUILayout.Button("Install TMP Package", GUILayout.Width(170), GUILayout.Height(30))) InstallConvaiUnityPackage(TMP_PACKAGE_PATH);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // IActiveBuildTargetChanged callback
        public int callbackOrder { get; }

        /// <summary>
        ///     Called when the active build target is changed.
        /// </summary>
        /// <param name="previousTarget">The previous build target.</param>
        /// <param name="newTarget">The new build target.</param>
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            // Check if the new build target is iOS and trigger the download of iOS DLL.
            if (newTarget == BuildTarget.iOS) TryToDownloadiOSDLL();
        }

        /// <summary>
        ///     Shows the Convai Custom Package Installer window.
        /// </summary>
        [MenuItem("Convai/Custom Package Installer", false, 10)]
        public static void ShowWindow()
        {
            ConvaiCustomPackageInstaller window = GetWindow<ConvaiCustomPackageInstaller>("Convai Custom Package Installer", true);
            window.minSize = new Vector2(500, 370);
            window.maxSize = window.minSize;
            window.titleContent.text = "Custom Package Installer";
            window.Show();
        }

        /// <summary>
        ///     Progress method to handle the installation/uninstallation progress.
        /// </summary>
        private void Progress()
        {
            Debug.Log("<color=cyan>Process in progress... Please wait.</color>");

            // Check if the request object is initialized
            if (_request == null) return;
            if (_request.IsCompleted)
            {
                switch (_request.Status)
                {
                    case StatusCode.InProgress:
                        // Do nothing while the request is still in progress
                        break;
                    case StatusCode.Success:
                        // Handle the successful completion of the package request
                        HandlePackageRequest();
                        break;
                    case StatusCode.Failure:
                        // Log an error message in case of failure
                        Debug.LogError("Error: " + _request.Error.message);
                        break;
                }

                // Remove the Progress method from the update event
                EditorApplication.UnlockReloadAssemblies();
                EditorApplication.update -= Progress;
            }
        }

        /// <summary>
        ///     Method to handle the completion of the package request.
        /// </summary>
        private void HandlePackageRequest()
        {
            switch (_currentSetup)
            {
                case SetupTypes.None:
                    // Do nothing for SetupTypes.None
                    break;
                case SetupTypes.ARiOS:
                    // Handle iOS AR package installation
                    HandleARPackageInstall();
                    Debug.Log("<color=lime>The request for package installation from the Package Manager has been successfully completed.</color>");
                    break;
                case SetupTypes.ARAndroid:
                    // Handle Android AR package installation
                    HandleARPackageInstall();
                    Debug.Log("<color=lime>The request for package installation from the Package Manager has been successfully completed.</color>");
                    break;
                case SetupTypes.VR:
                    // Handle VR package installation
                    HandleVRPackageInstall();
                    Debug.Log("<color=lime>The request for package installation from the Package Manager has been successfully completed.</color>");
                    break;
                case SetupTypes.Uninstaller:
                    // Handle uninstallation package completion
                    HandleUninstallPackage();
                    Debug.Log("<color=lime>The request for package uninstallation from the Package Manager has been successfully completed.</color>");
                    break;
            }

            // Add the Progress method back to the update event
            EditorApplication.update += Progress;
        }

        /// <summary>
        ///     Method to handle the uninstallation of packages.
        /// </summary>
        private void HandleUninstallPackage()
        {
            // Check if the request object is not initialized
            if (_request == null)
            {
                // Define asset paths to delete
                string[] deleteAssetPaths =
                {
                    "Assets/Samples",
                    "Assets/Convai/ConvaiAR",
                    "Assets/Convai/ConvaiVR",
                    "Assets/XR",
                    "Assets/XRI"
                };

                List<string> outFailedPaths = new();
                // Delete specified asset paths
                AssetDatabase.DeleteAssets(deleteAssetPaths, outFailedPaths);

                // Log errors if any deletion fails
                if (outFailedPaths.Count > 0)
                    foreach (string failedPath in outFailedPaths)
                        Debug.LogError("Failed to delete : " + failedPath);
            }

            // Define package names for uninstallation
            string ARCorePackageName = "com.unity.xr.arcore";
            string ARKitPackageName = "com.unity.xr.arkit";
            string OpenXRPackageName = "com.unity.xr.openxr";
            string XRInteractionToolkitPackageName = "com.unity.xr.interaction.toolkit";

            // Check if ARCore is installed and initiate removal
            if (IsPackageInstalled(ARCorePackageName)) _request = Client.Remove(ARCorePackageName);

            // Check if ARKit is installed and initiate removal
            if (IsPackageInstalled(ARKitPackageName))
            {
                _request = Client.Remove(ARKitPackageName);
            }
            // Check if OpenXR is installed and initiate removal
            else if (IsPackageInstalled(OpenXRPackageName))
            {
                _request = Client.Remove(OpenXRPackageName);
            }
            // Check if XR Interaction Toolkit is installed and initiate removal
            else if (IsPackageInstalled(XRInteractionToolkitPackageName))
            {
                _request = Client.Remove(XRInteractionToolkitPackageName);
            }
            else
            {
                // Stop the update event if the request is not initialized
                EditorApplication.update -= Progress;
                EditorApplication.UnlockReloadAssemblies();
            }

            // Remove the Progress method from the update event if the request is not initialized
            if (_request == null) EditorApplication.update -= Progress;
        }

        /// <summary>
        ///     Method to start the installation of a specific package setup.
        /// </summary>
        private void StartPackageInstallation(SetupTypes setupType)
        {
            // Log a message indicating the start of the package installation
            Debug.Log($"<color=cyan>Installation of {setupType} package has started... This process may take 3-5 minutes.</color>");

            // Warn the user about the possibility of 'Failed to Resolve Packages' error
            Debug.LogWarning("<color=yellow>If you encounter with 'Failed to Resolve Packages' error, there's no need to be concerned.</color>");

            // Reset the package installation index
            _currentPackageInstallIndex = 0;

            // Set the current setup type
            _currentSetup = setupType;
            // Initialize the Universal Render Pipeline (URP) setup
            InitializeURPSetup();
        }

        /// <summary>
        ///     Method to handle the installation of AR-related packages.
        /// </summary>
        private void HandleARPackageInstall()
        {
            // Check the current package installation index
            if (_currentPackageInstallIndex == 0)
            {
                switch (_currentSetup)
                {
                    case SetupTypes.ARAndroid:
                        // Initialize the ARCore setup
                        InitializeARCoreSetup();
                        break;
                    case SetupTypes.ARiOS:
                        // Initialize the ARKit setup
                        InitializeARKitSetup();
                        break;
                }
            }
            else
            {
                // Install AR-related packages and perform necessary setup
                InstallConvaiUnityPackage(AR_PACKAGE_PATH);
                InstallConvaiUnityPackage(URP_CONVERTER_PACKAGE_PATH);
                switch (_currentSetup)
                {
                    case SetupTypes.ARAndroid:
                        TryToChangeEditorBuildTargetToAndroid();
                        break;
                    case SetupTypes.ARiOS:
                        TryToChangeEditorBuildTargetToiOS();
                        break;
                }
            }
        }

        /// <summary>
        ///     Method to handle the installation of VR-related packages.
        /// </summary>
        private void HandleVRPackageInstall()
        {
            // Check the current package installation index
            if (_currentPackageInstallIndex == 0)
            {
                // Initialize the OpenXR setup
                InitializeOpenXRSetup();
            }
            else if (_currentPackageInstallIndex == 1)
            {
                // Initialize the XR Interaction Toolkit setup
                InitializeXRInteractionToolkitSetup();
            }
            else
            {
                // Install VR-related packages and perform necessary setup
                InstallConvaiUnityPackage(VR_PACKAGE_PATH);
                InstallConvaiUnityPackage(URP_CONVERTER_PACKAGE_PATH);
                TryToChangeEditorBuildTargetToAndroid();
            }
        }

        /// <summary>
        ///     Method to initialize the URP setup.
        /// </summary>
        private void InitializeURPSetup()
        {
            // Define the URP package name
            const string URPPackageName = "com.unity.render-pipelines.universal@14.0.11";

            // Check if the URP package is already installed
            if (IsPackageInstalled(URPPackageName))
            {
                // If installed, handle the successful package request
                HandlePackageRequest();
                return;
            }

            // If not installed, send a request to the Package Manager to add the URP package
            _request = Client.Add(URPPackageName);
            Debug.Log($"<color=orange>{URPPackageName} Package Installation Request Sent to Package Manager.</color>");

            // Add the Progress method to the update event to monitor the installation progress
            EditorApplication.update += Progress;
        }

        /// <summary>
        ///     Method to initialize the ARCore setup.
        /// </summary>
        private void InitializeARCoreSetup()
        {
            // Set the current package installation index for ARCore
            _currentPackageInstallIndex = 1;

            // Define the ARCore package name
            string ARCorePackageName = "com.unity.xr.arcore@5.1.4";

            // Check if the ARCore package is already installed
            if (IsPackageInstalled(ARCorePackageName))
            {
                // If installed, handle the AR package installation
                HandleARPackageInstall();
                return;
            }

            // If not installed, send a request to the Package Manager to add the ARCore package
            _request = Client.Add(ARCorePackageName);
            Debug.Log($"<color=orange>{ARCorePackageName} Package Installation Request sent to Package Manager.</color>");
        }

        /// <summary>
        ///     Method to initialize the ARKit setup.
        /// </summary>
        private void InitializeARKitSetup()
        {
            // Set the current package installation index for AR Setup
            _currentPackageInstallIndex = 1;

            // Define the ARKit package name
            string ARKitPackageName = "com.unity.xr.arkit@5.1.4";

            // Check if the ARKit package is already installed
            if (IsPackageInstalled(ARKitPackageName))
            {
                // If installed, handle the AR package installation
                HandleARPackageInstall();
                return;
            }

            // If not installed, send a request to the Package Manager to add the ARKit package
            _request = Client.Add(ARKitPackageName);
            Debug.Log($"<color=orange>{ARKitPackageName} Package Installation Request sent to Package Manager.</color>");
        }

        /// <summary>
        ///     Method to initialize the OpenXR setup.
        /// </summary>
        private void InitializeOpenXRSetup()
        {
            // Set the current package installation index for OpenXR
            _currentPackageInstallIndex = 1;

            // Define the OpenXR package name
            string OpenXRPackageName = "com.unity.xr.openxr@1.10.0";

            // Check if the OpenXR package is already installed
            if (IsPackageInstalled(OpenXRPackageName))
            {
                // If installed, handle the VR package installation
                HandleVRPackageInstall();
                return;
            }

            // If not installed, send a request to the Package Manager to add the OpenXR package
            _request = Client.Add(OpenXRPackageName);
            Debug.Log($"<color=orange>{OpenXRPackageName} Package Installation Request sent to Package Manager.</color>");
        }

        /// <summary>
        ///     Method to initialize the XR Interaction Toolkit setup.
        /// </summary>
        private void InitializeXRInteractionToolkitSetup()
        {
            // Set the current package installation index for XR Interaction Toolkit
            _currentPackageInstallIndex = 2;

            // Define the XR Interaction Toolkit package name
            string XRInteractionToolkitPackageName = "com.unity.xr.interaction.toolkit@2.5.4";

            // Check if the XR Interaction Toolkit package is already installed
            if (IsPackageInstalled(XRInteractionToolkitPackageName))
            {
                // If installed, handle the VR package installation
                HandleVRPackageInstall();
                return;
            }

            // If not installed, send a request to the Package Manager to add the XR Interaction Toolkit package
            _request = Client.Add(XRInteractionToolkitPackageName);
            Debug.Log($"<color=orange>{XRInteractionToolkitPackageName} Package Installation Request sent to Package Manager.</color>");
        }

        /// <summary>
        ///     Method to install a custom Convai Unity package.
        /// </summary>
        private void InstallConvaiUnityPackage(string packagePath)
        {
            // Import the Unity package
            AssetDatabase.ImportPackage(packagePath, false);

            // Get the package name without extension
            string packageName = Path.GetFileNameWithoutExtension(packagePath);
            Debug.Log($"<color=lime>{packageName} Custom Unity Package Installation Completed.</color>");
        }

        /// <summary>
        ///     Method to check if a package is already installed.
        /// </summary>
        private bool IsPackageInstalled(string packageName)
        {
            // Iterate through all registered packages
            foreach (PackageInfo packageInfo in PackageInfo.GetAllRegisteredPackages())
                // Check if the package name matches
                if (packageInfo.name == packageName)
                    // Return true if the package is installed
                    return true;

            // Return false if the package is not installed
            return false;
        }

        /// <summary>
        ///     Try changing the editor build target to Android.
        /// </summary>
        private void TryToChangeEditorBuildTargetToAndroid()
        {
            // Check if the current build target is not Android
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                // Switch the active build target to Android
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
                Debug.Log("<color=lime>Build Target Platform is being Changed to Android...</color>");
            }
        }

        /// <summary>
        ///     Try changing the editor build target to iOS.
        /// </summary>
        private void TryToChangeEditorBuildTargetToiOS()
        {
            // Check if the current build target is not iOS
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                // Switch the active build target to iOS
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.iOS, BuildTarget.iOS);
                Debug.Log("<color=lime>Build Target Platform is being Changed to iOS...</color>");
            }
        }

        /// <summary>
        ///     Attempts to download the iOS DLL using the IOSDLLDownloader class.
        /// </summary>
        private void TryToDownloadiOSDLL()
        {
            // Call the TryToDownload method from the IOSDLLDownloader class.
            iOSDLLDownloader.TryToDownload();
        }
    }
}
#endif