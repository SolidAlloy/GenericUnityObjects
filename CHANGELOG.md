## [2.17.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.17.0...2.17.1) (2022-08-02)


### Bug Fixes

* Fixed exception when creating a new unity object through Creatable attribute ([99ce2de](https://github.com/SolidAlloy/GenericUnityObjects/commit/99ce2de964c97b68221d75b94238fcda9268e8ce))
* Fixed incorrect concrete types used for creating generic scriptable objects ([07c73ae](https://github.com/SolidAlloy/GenericUnityObjects/commit/07c73ae737869aae22e9b65667d303276b4b0b0a))

# [2.17.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.16.0...2.17.0) (2022-07-24)


### Bug Fixes

* Fixed concrete classes of a different type added to type database after a generic unity-object class is removed ([4d07108](https://github.com/SolidAlloy/GenericUnityObjects/commit/4d07108faecf28241d9ffa42a500c469068d3f83))
* Fixed exception in Reference property drawer ([1602288](https://github.com/SolidAlloy/GenericUnityObjects/commit/16022888149db226df01c610875ebb5bd400c6c3))


### Features

* Implemented Creatable attribute for generic MonoBehaviours ([d3420ac](https://github.com/SolidAlloy/GenericUnityObjects/commit/d3420ac50647dfc1f23f1e1144856baabbb89803))
* Switched from GUID to assembly names in asmdefs ([349d9c1](https://github.com/SolidAlloy/GenericUnityObjects/commit/349d9c13c117a4041fa54661e3f26ce6c387dffa))

# [2.16.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.15.3...2.16.0) (2022-06-19)


### Bug Fixes

* Fixed compilation error related to AssetDatabase.SaveAssetIfDirty() in Unity 2021.1.16 and older ([4c2ac45](https://github.com/SolidAlloy/GenericUnityObjects/commit/4c2ac45058aeaad6e9a6afe8bc6a750178cb0e51))
* Improved the way the package detects if the platform can emit code at runtime ([47877e7](https://github.com/SolidAlloy/GenericUnityObjects/commit/47877e75f0c8f7afc5ebbe47495c112008219340))
* Removed a warning regarding pdb files in Unity on non-Windows platforms ([7686dbe](https://github.com/SolidAlloy/GenericUnityObjects/commit/7686dbe06baebc52733f84a69dc964e684aad024))


### Features

* Made auto-generated classes public so that they can be referenced by code generation plugins ([0bd4e67](https://github.com/SolidAlloy/GenericUnityObjects/commit/0bd4e673547f6b3514e791efb85473050325ef33))
* Made possible to inherit from simple ScriptableObject instead of GenericScriptableObject to create generic assets ([0c4ac2d](https://github.com/SolidAlloy/GenericUnityObjects/commit/0c4ac2d9b1b63e3313639d7c02296c23421337e8))

## [2.15.3](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.15.2...2.15.3) (2022-05-08)


### Bug Fixes

* Fixed errors in dependencies and updated them ([0eb43b1](https://github.com/SolidAlloy/GenericUnityObjects/commit/0eb43b1e3ef65221dfd014da4d953eb4ed13011f))

## [2.15.2](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.15.1...2.15.2) (2022-03-13)


### Bug Fixes

* Fixed generic scriptable object not being created through the Creatable attribute for the first time ([b3be59f](https://github.com/SolidAlloy/GenericUnityObjects/commit/b3be59fb404b642a1ab61e8620292d3b4b3afc0e))

## [2.15.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.15.0...2.15.1) (2022-03-08)


### Bug Fixes

* Fixed the missing reference to MissingScriptType ([1f9483b](https://github.com/SolidAlloy/GenericUnityObjects/commit/1f9483baa99a8deb4b1bfb76b2b7cd322a89f065))

# [2.15.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.14.1...2.15.0) (2022-02-13)


### Bug Fixes

* Fixed missing dependency on EasyButtons ([cdee47c](https://github.com/SolidAlloy/GenericUnityObjects/commit/cdee47c28a7c25f6e4200676e112ef33449e1407))


### Features

* Added integration with MissingScriptType ([274fd76](https://github.com/SolidAlloy/GenericUnityObjects/commit/274fd7669da460f01512e7798a557f30ca08ef6a))
* Made ProjectSettings public ([31d9bdc](https://github.com/SolidAlloy/GenericUnityObjects/commit/31d9bdc275f37a6b8b1f4e672ac51dbacfdf82cb))

## [2.14.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.14.0...2.14.1) (2022-02-03)


### Bug Fixes

* Fixed the error in console regarding the immutable Changelog file ([d6df2ae](https://github.com/SolidAlloy/GenericUnityObjects/commit/d6df2ae6527eb15eff94c4aea3ff1c66a7b6db2d))

# [2.14.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.13.1...2.14.0) (2022-02-03)


### Bug Fixes

* Fixed ArgumentException occuring on domain reload ([084605a](https://github.com/SolidAlloy/GenericUnityObjects/commit/084605a43e79daa14896ffcd20cb4672d2101e67))
* Fixed changes to the database not saved after removing a concrete class ([d3e5314](https://github.com/SolidAlloy/GenericUnityObjects/commit/d3e5314ce61a164fde94eeded02eae008c855529))
* Fixed directory-not-found exception when using the package for the first time ([629be74](https://github.com/SolidAlloy/GenericUnityObjects/commit/629be746196417763a2b64204a34dc6506cb4c92))
* Fixed FileNotFound exception when trying to replace an assembly that does not exist ([f090bb0](https://github.com/SolidAlloy/GenericUnityObjects/commit/f090bb08447c7dbe0ff737220abf1142b75e72b1))
* Fixed generic components not generating and Undo record when being added ([b58e0d8](https://github.com/SolidAlloy/GenericUnityObjects/commit/b58e0d8e353416e67863e325cb043fb47d68b616))
* Fixed not working custom editor for generic MonoBehaviours with new versions of Odin Inspector ([e3ad7e6](https://github.com/SolidAlloy/GenericUnityObjects/commit/e3ad7e639c2d6e0ba4f561c22efd080ecc862db1))
* Fixed type exception when drawing a non-generic type object field using GenericObjectField ([74da9ca](https://github.com/SolidAlloy/GenericUnityObjects/commit/74da9cad1dca219ce715f7a4ce679336fd54b7b7))
* Made GenericObjectDrawer recognize that the current target is generic and show it correctly even if the passed type is not generic ([e16bcf7](https://github.com/SolidAlloy/GenericUnityObjects/commit/e16bcf7b9777daf98ecda006ff0d313933005479))


### Features

* Added a namespace to the auto-generated concrete classes ([73af6e8](https://github.com/SolidAlloy/GenericUnityObjects/commit/73af6e8a00cb93c40e87f8328f72e8c92f4e0ce0))
* Added GenericBehaviourCreator.AddComponent() overload that tells if the domain reload is needed ([a781e1f](https://github.com/SolidAlloy/GenericUnityObjects/commit/a781e1fd490f66afd2c5ee0f5547d5514f303aae))
* Replaced a centered dropdown window with opening it at the mouse position when creating a generic scriptable object ([2932b77](https://github.com/SolidAlloy/GenericUnityObjects/commit/2932b77c2089934d1cc8201233470bea806bfc86))
* Started regenerating selector assemblies that were removed ([821c8e2](https://github.com/SolidAlloy/GenericUnityObjects/commit/821c8e2a480cc1e6a298009aaee2c9c780e82ff1))

## [2.13.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.13.0...2.13.1) (2021-11-21)


### Bug Fixes

* Updated SolidUtilities to the required version ([735a0d0](https://github.com/SolidAlloy/GenericUnityObjects/commit/735a0d08caa79bc06771c3fba9dfe938433a2f40))

# [2.13.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.12.1...2.13.0) (2021-11-21)


### Bug Fixes

* Fixed NullReferenceException when generating a new concrete class ([c8f0a83](https://github.com/SolidAlloy/GenericUnityObjects/commit/c8f0a833eb771c9f12ec09a52ab30be662067934))
* Removed the deprecated warning when setting custom icons ([b104d91](https://github.com/SolidAlloy/GenericUnityObjects/commit/b104d910740df149718259d2c4f816eee65d4791))
* Started creating directories only when they are needed instead of creating them beforehand ([e425f32](https://github.com/SolidAlloy/GenericUnityObjects/commit/e425f328dbd3c12f3b42e9852eeb4b6d24a4b653))


### Features

* Added PackageUpgrade to move the generated assemblies to their new directories ([ffb4388](https://github.com/SolidAlloy/GenericUnityObjects/commit/ffb438829be92f7bbdf8fc37870737fb77a13f02))
* Started detecting and adding back generated assemblies whose information was missing from the database ([ef33dbf](https://github.com/SolidAlloy/GenericUnityObjects/commit/ef33dbfaeef6730f93a75321687ea409c0f8ffeb))

## [2.12.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.12.0...2.12.1) (2021-11-16)


### Bug Fixes

* Updated the ClassTypeReference dependency ([9f80a4e](https://github.com/SolidAlloy/GenericUnityObjects/commit/9f80a4e3a01647acade01cee562b4cc1ae5f19f5))

# [2.12.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.11.4...2.12.0) (2021-11-16)


### Bug Fixes

* Fixed MissingFieldException when using the plugin in Unity 2021.2 ([5c61cf3](https://github.com/SolidAlloy/GenericUnityObjects/commit/5c61cf3451f78e224aa4dbf9031cc90e9caa8ca2))


### Features

* Adapted the plugin for NET Standard 2.1 ([f5f885f](https://github.com/SolidAlloy/GenericUnityObjects/commit/f5f885fae6e7ae1748c7c9910bdfbdcd3c7e927d))
* Added z_ prefix to all the generated assemblies to keep them at the bottom of dropdowns where DLL files are listed ([1e6652a](https://github.com/SolidAlloy/GenericUnityObjects/commit/1e6652a1fea1f0c6c41d31f18b9896f0a143426f))
* Stopped generating .mdb files ([6fe50ed](https://github.com/SolidAlloy/GenericUnityObjects/commit/6fe50ed4a625c569f36435f7cf6485fc5eaf9a0b))

## [2.11.4](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.11.3...2.11.4) (2021-10-24)


### Bug Fixes

* Fixed suffixes being added to generated assemblies names when they are updated ([4f79d82](https://github.com/SolidAlloy/GenericUnityObjects/commit/4f79d822b6b016d58910413ef7bbed2b76bcf0ec))
* Started adding back generated assemblies that exist in the folder but are missing in the database ([1e88e3c](https://github.com/SolidAlloy/GenericUnityObjects/commit/1e88e3cd93882f5ec8c89b6cad437b967eee0197))

## [2.11.3](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.11.2...2.11.3) (2021-10-22)


### Bug Fixes

* Fixed NRE when opening a type dropdown on MacOS ([8e3a0d8](https://github.com/SolidAlloy/GenericUnityObjects/commit/8e3a0d87e2ae58934883ecc2eab4aebfb07ef8f4))

## [2.11.2](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.11.1...2.11.2) (2021-10-17)


### Bug Fixes

* Fixed NRE when choosing a type for a new generic unity object ([820f3b1](https://github.com/SolidAlloy/GenericUnityObjects/commit/820f3b1d158c033295bc5ee424f22e2fd451407a))

## [2.11.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.11.0...2.11.1) (2021-10-16)


### Bug Fixes

* Fixed MissingMethodException when choosing a UnityEvent method in Unity 2020.3.16 and earlier ([896dcfe](https://github.com/SolidAlloy/GenericUnityObjects/commit/896dcfe0c0f143ef7ba7ce692cc70a13a492e56b))

# [2.11.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.6...2.11.0) (2021-10-13)


### Features

* Added an ability to traverse the hierarchy of types in the dropdown with keyboard ([1f8895d](https://github.com/SolidAlloy/GenericUnityObjects/commit/1f8895db60ad3058a819f24766d4e91f6a5757a6))

## [2.10.6](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.5...2.10.6) (2021-10-07)


### Bug Fixes

* Fixed missing method exception in Unity 2020.3.15 and older ([848005c](https://github.com/SolidAlloy/GenericUnityObjects/commit/848005c47ec8e9d7bef80fdfed05dddebc033cc4))

## [2.10.5](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.4...2.10.5) (2021-10-03)


### Bug Fixes

* Fixed "Type is not a supported pptr value" error when using UnityEvents ([f3a6804](https://github.com/SolidAlloy/GenericUnityObjects/commit/f3a680466e0a9792e94e7ba79af91a6039d1c9c5))
* Fixed ApplicationException occurring sometimes after assembly reload ([0d56d5e](https://github.com/SolidAlloy/GenericUnityObjects/commit/0d56d5eed09b5f3fa3755228932f8435134e966a))

## [2.10.4](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.3...2.10.4) (2021-09-30)


### Bug Fixes

* Started saving config changes to disk immediately after a change in generated assemblies ([1cec201](https://github.com/SolidAlloy/GenericUnityObjects/commit/1cec201c6311148470403ea6dceb68a2a76ff40c))

## [2.10.3](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.2...2.10.3) (2021-09-29)


### Bug Fixes

* Fixed MissingReferenceException sometimes appearing on MacOS ([06f5298](https://github.com/SolidAlloy/GenericUnityObjects/commit/06f5298c6eb9dc15ec0b61e217d7c5d6d27c3daa))

## [2.10.2](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.1...2.10.2) (2021-09-27)


### Bug Fixes

* Fixed ArgumentNullException after script reloading ([0486764](https://github.com/SolidAlloy/GenericUnityObjects/commit/0486764b6c9883cfde37ff8b1e81bc21ebe43b76))
* Fixed incorrect type name when using UnityEvent with GenericScriptableObject ([a67f794](https://github.com/SolidAlloy/GenericUnityObjects/commit/a67f794d71a9931afbfbfc9e3c88ae64c0f19d7c))

## [2.10.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.10.0...2.10.1) (2021-09-26)


### Bug Fixes

* Fixed the correct type not being found in MonoScript if the class was not the first in the file ([a1edfe5](https://github.com/SolidAlloy/GenericUnityObjects/commit/a1edfe52797451d2b42460cbd0122067288b29c1))

# [2.10.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.9.0...2.10.0) (2021-09-16)


### Features

* Added a checkbox in project settings allowing to put the + button next to all GenericScriptableObject fields regardless of the Creatable attribute ([5caf05b](https://github.com/SolidAlloy/GenericUnityObjects/commit/5caf05bf4cc973d5955ed2524a877934d1c6d95d))
* Fixed an issue with selector component not being recognized after pulling changes from git repo ([2026c76](https://github.com/SolidAlloy/GenericUnityObjects/commit/2026c76cea23a05d3b1cf56d5d6a2307b293beaa))
* Made responses in the unity event drawer draggable ([8292067](https://github.com/SolidAlloy/GenericUnityObjects/commit/8292067fa623601b54d55dc6a2ac495793f3bc76))

# [2.9.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.8.1...2.9.0) (2021-09-05)


### Bug Fixes

* Added automatic recompilation when an asset is not recognized ([73fce44](https://github.com/SolidAlloy/GenericUnityObjects/commit/73fce440a656fe359b8f8208bd8db14409809624))
* Fixed an issue with databases being emptied when a Unity project is first opened after cloning ([4d1e94d](https://github.com/SolidAlloy/GenericUnityObjects/commit/4d1e94d2c46567ea15a78e8157e3547da162a4ed))
* Fixed compilation key not being removed on editor quit ([bb84de2](https://github.com/SolidAlloy/GenericUnityObjects/commit/bb84de2ada7fd70e18205f57f58d25639af4777b))
* Fixed CompilationHelper not executing scripts reload when needed ([9fdc02c](https://github.com/SolidAlloy/GenericUnityObjects/commit/9fdc02c2a2b3eb836b3f4362dcd067dbaf0d28ac))
* Fixed NullReferenceException when there are broken assemblies ([ed9bff2](https://github.com/SolidAlloy/GenericUnityObjects/commit/ed9bff2f5b4400b97301e8bbaa6760ad881ee3bd))
* Moved compilation-related fields of PersistentStorage to PlayerPrefs ([96b0bfc](https://github.com/SolidAlloy/GenericUnityObjects/commit/96b0bfcbaa2e69efc0884c4e74f929b11542cfb3))
* Started reimporting assemblies that failed to import properly the first time ([07ca05e](https://github.com/SolidAlloy/GenericUnityObjects/commit/07ca05e0939c5f86e20525d94e56a1803b62f450))
* Started reimporting created assets along with the corresponding assemblies when a MonoScript is lost ([f2cf56d](https://github.com/SolidAlloy/GenericUnityObjects/commit/f2cf56db16b78627741a4beb624c0bd8c98e5264))
* Started reimporting created assets of failed assemblies on next domain reload ([583f74b](https://github.com/SolidAlloy/GenericUnityObjects/commit/583f74be1f7b34553058a93af855ed3ea6e7e23b))
* Started skipping assembly reimport if we were not able to fix the assembly previously ([5e75444](https://github.com/SolidAlloy/GenericUnityObjects/commit/5e75444b28b9c5966e42f554d3623545b6a2333b))


### Features

* Added PlayerPrefs overloads for PersistentStorage save-loading methods ([2247f7b](https://github.com/SolidAlloy/GenericUnityObjects/commit/2247f7b12a94374441e1dc8fa831ef5656e07dc5))

## [2.8.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.8.0...2.8.1) (2021-09-02)


### Bug Fixes

* Fixed generic scriptable objects with multiple type arguments having incorrect constraints when choosing generic arguments ([3eb0419](https://github.com/SolidAlloy/GenericUnityObjects/commit/3eb0419e8eae3abc1709d3d848803c75fb7d63ad))

# [2.8.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.7.1...2.8.0) (2021-08-31)


### Bug Fixes

* Fixed NullReferenceException when adding new generic monobehaviour components ([03f17bf](https://github.com/SolidAlloy/GenericUnityObjects/commit/03f17bf78ad1056bd35816399ab9dabdaa304b44))


### Features

* Started drawing easy buttons in generic unityobjects inspectors if the package is installed ([ed0c74a](https://github.com/SolidAlloy/GenericUnityObjects/commit/ed0c74a685aa7a0d22fc0908eea96eac55e61621))

## [2.7.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.7.0...2.7.1) (2021-08-30)


### Bug Fixes

* Fixed constant reloading when importing a generic type with a GUID reference to an icon but no texture in the project ([2698e47](https://github.com/SolidAlloy/GenericUnityObjects/commit/2698e475b95b75b27dc3c5ebc8e1507767161045))

# [2.7.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.6.0...2.7.0) (2021-08-28)


### Bug Fixes

* Added a verbose warning message if a MonoScript is not found inside a generated assembly ([6cf4d08](https://github.com/SolidAlloy/GenericUnityObjects/commit/6cf4d088e24399afb91353d0a835b33a7dbc3048))


### Features

* Added ConcreteClassAdded event to ConcreteClassCreator ([e33f4b8](https://github.com/SolidAlloy/GenericUnityObjects/commit/e33f4b8a9759b0bb68291d02efee5aa770dd9e58))
* Added new GenericScriptableArchitecture assemblies to InternalsVisibleTo ([91fabe0](https://github.com/SolidAlloy/GenericUnityObjects/commit/91fabe0055a184e7b1890059b59581eedde2a33a))
* Added ObjectSelectorHelper.ShowGenericSelector() method ([387899f](https://github.com/SolidAlloy/GenericUnityObjects/commit/387899fdaacb5b14b161fe9e22a9912829ae860b))
* Added the ApplyToChildren attribute ([9ade420](https://github.com/SolidAlloy/GenericUnityObjects/commit/9ade420dedde066a9d75c1b30e1eb1873702de52))

# [2.6.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.5.0...2.6.0) (2021-08-22)


### Bug Fixes

* Added TEST_FRAMEWORK define symbol to unity test assemblies ([73ecc9d](https://github.com/SolidAlloy/GenericUnityObjects/commit/73ecc9d0a561c948130df5343b6619b80da03b41))
* Fixed ArgumentOutOfRange exception when trying to find a custom script icon ([464e043](https://github.com/SolidAlloy/GenericUnityObjects/commit/464e043b2314849243bb0cd20165d204339c2daf))
* Fixed attempt to generate a concrete class while it already exists when a generic scriptable object is created ([167e87c](https://github.com/SolidAlloy/GenericUnityObjects/commit/167e87c3dae6cd13d0546c205cbfb3f674f57012))
* Fixed long delay before the interactive creation of a scriptable object ([d3f9e4f](https://github.com/SolidAlloy/GenericUnityObjects/commit/d3f9e4fd0f2d753f0002ba567cb6d1c1625b0181))
* Fixed some generic types being regenerated when Unity starts with compilation errors ([920cbe1](https://github.com/SolidAlloy/GenericUnityObjects/commit/920cbe1b3683c95b57aefcc196e0481b27c60d09))
* Removed unnecessary Assembly-CSharp inclusion in type dropdown ([c4e1663](https://github.com/SolidAlloy/GenericUnityObjects/commit/c4e1663c410ce885ba432162f5bd4789ec5fdf34))
* Updated according to changes in TypeReferences ([a8d1883](https://github.com/SolidAlloy/GenericUnityObjects/commit/a8d18830fc14d51f8345e25abf50003137a902fc))


### Features

* Added ability to assign the created asset after assembly recompilation ([feab545](https://github.com/SolidAlloy/GenericUnityObjects/commit/feab545a640304cef31d141035461e3746001021))
* Added object pinging in the hierarchy when an object is created through the Creatable attribute ([4f45bd1](https://github.com/SolidAlloy/GenericUnityObjects/commit/4f45bd1b647087452fd5e717b271224d000eacd6))
* Added the creatable attribute that adds a Create button in the inspector next to object field ([cde98f2](https://github.com/SolidAlloy/GenericUnityObjects/commit/cde98f2d981701db3ee72d2fba93f8fc05a94184))

# [2.5.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.10...2.5.0) (2021-06-20)


### Bug Fixes

* Changed how generic UnityObjects are drawn with Odin ([681eea7](https://github.com/SolidAlloy/GenericUnityObjects/commit/681eea703c4c7a19a036b3fa92c4a2a017880041))


### Features

* Added GenericSOCreator.CreateAssetAtPath method ([38ec365](https://github.com/SolidAlloy/GenericUnityObjects/commit/38ec36577b2dd2b15088284b10c957a9609b3f3d))

## [2.4.10](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.9...2.4.10) (2021-04-30)


### Bug Fixes

* Fixed custom icons not being set for generated classes in Unity 2021 ([8dde5cb](https://github.com/SolidAlloy/GenericUnityObjects/commit/8dde5cbcce4aefe1836ebe40355b1a196ff8daf1))
* Fixed generic types databases not being initialized sometimes ([0e65d84](https://github.com/SolidAlloy/GenericUnityObjects/commit/0e65d84a5aafe8fd653ff15317ef152b6fcd2936))
* Fixed missing type exception when drawing a generic object selector in Unity 2020 ([f301bda](https://github.com/SolidAlloy/GenericUnityObjects/commit/f301bda6d0aca61aac3ea2e3b886532afc808b7b))
* Fixed MissingMethodException when using Unity 2021 ([5289e7c](https://github.com/SolidAlloy/GenericUnityObjects/commit/5289e7c5c5ffd2069ae3ce6f599d0fc5b69a0dca))
* Fixed NullReferenceException when entering PlayMode with PlayModeOptions disabled ([fc85fdf](https://github.com/SolidAlloy/GenericUnityObjects/commit/fc85fdf8d39915627eb91f37da15fb15a31e4779))
* Replaced the wrong using statement in IconFinder ([b48c844](https://github.com/SolidAlloy/GenericUnityObjects/commit/b48c844385c6b4be74906d6f0a1976d32441208a))

## [2.4.9](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.8...2.4.9) (2021-04-01)


### Bug Fixes

* Fixed GUIContent null error when using GenericObjectDrawer.ObjectField with a non-generic UnityEngine.Object instance ([bd71fa6](https://github.com/SolidAlloy/GenericUnityObjects/commit/bd71fa61920babd938535baa3ef31d216c226382))

## [2.4.8](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.7...2.4.8) (2021-03-23)


### Bug Fixes

* Adjusted integration tests to the new OneTypeSelectionWindow class ([dd317ab](https://github.com/SolidAlloy/GenericUnityObjects/commit/dd317ab123cbc99353ef5e0bae62fca99bc63ef3))
* Made OnTypeSelectionWindow a POCO that instantiates dropdown window as a popup ([200b34e](https://github.com/SolidAlloy/GenericUnityObjects/commit/200b34ee9500502f4141ee8320f14594829337ef))
* Updated the Type References dependency ([24178a5](https://github.com/SolidAlloy/GenericUnityObjects/commit/24178a574c5317a34a232ae6922384b06b795992))

## [2.4.7](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.6...2.4.7) (2021-03-19)


### Bug Fixes

* Fixed custom Odin drawers drawing non-generic scriptable objects ([4b892e4](https://github.com/SolidAlloy/GenericUnityObjects/commit/4b892e462963b79c0f01cc4f4216c639e869ebd3))

## [2.4.6](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.5...2.4.6) (2021-03-01)


### Bug Fixes

* Made GenericUnityObjectDrawer override the default behaviour of Odin ([ec518fd](https://github.com/SolidAlloy/GenericUnityObjects/commit/ec518fd3bd7273418b24d01dfd073c9e2d85e871))

## [2.4.5](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.4...2.4.5) (2021-02-23)


### Bug Fixes

* If a child type object is assigned to a generic object field that requires a parent type, the child type will be shown instead of parent ([1b0d432](https://github.com/SolidAlloy/GenericUnityObjects/commit/1b0d432f6a50af9fba14ea66644f65672d7001a2))

## [2.4.4](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.3...2.4.4) (2021-02-22)


### Bug Fixes

* Fixed compilation error on build in TypeCache.GetTypesDerivedFrom() ([e6a3653](https://github.com/SolidAlloy/GenericUnityObjects/commit/e6a3653e99f5f4fa86e06f0e69d743be73fb72cf))

## [2.4.3](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.2...2.4.3) (2021-02-20)


### Bug Fixes

* Added constraints to test assembly definitions ([4ae82a0](https://github.com/SolidAlloy/GenericUnityObjects/commit/4ae82a03251590e2a09275af5f20ae455f85004e))

## [2.4.2](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.1...2.4.2) (2021-02-19)


### Bug Fixes

* Replaced the broken dependency dll ([4623aef](https://github.com/SolidAlloy/GenericUnityObjects/commit/4623aef2783fe3ae99dba5f8eb99bee2c277394b))

## [2.4.1](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.4.0...2.4.1) (2021-02-19)


### Bug Fixes

* Fixed compilation errors occurring in dependencies ([7883825](https://github.com/SolidAlloy/GenericUnityObjects/commit/7883825776e1572d516652973a76f9942e95a710))

# [2.4.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.3.0...2.4.0) (2021-02-15)


### Bug Fixes

* Fixed a few errors in the unit tests ([58663ce](https://github.com/SolidAlloy/GenericUnityObjects/commit/58663ce2fc8fd8c1f10249b824d2f6a7379228fd))
* Fixed built-in types not being replaced when displaying generic type name ([f539141](https://github.com/SolidAlloy/GenericUnityObjects/commit/f539141fda421540481ba4cb6f21723b5d88772c))
* Fixed interactive renaming of a scriptable object stopping immediately ([f29f6e0](https://github.com/SolidAlloy/GenericUnityObjects/commit/f29f6e0ece574bb9775049e95c3a07eafd291286))
* Started updating asset title if it was renamed. ([066308f](https://github.com/SolidAlloy/GenericUnityObjects/commit/066308fe908f30b1d3a0d389a1eebd20a7e0e95a))


### Features

* Made generated DLLs respect the custom icons set in generic scripts ([17b9073](https://github.com/SolidAlloy/GenericUnityObjects/commit/17b907357855b9c7f4a15021e9dfd97d88684c03))

# [2.3.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.2.0...2.3.0) (2021-02-08)


### Bug Fixes

* Fixed missing method signatures in GenericUnityEditorInternals ([37795f5](https://github.com/SolidAlloy/GenericUnityObjects/commit/37795f555401e326f945a950f286df76fd26b9c8))


### Features

* Made generic components show up correctly in UnityEvent UI. ([da223f2](https://github.com/SolidAlloy/GenericUnityObjects/commit/da223f2dd08b1fe105b6c13ed25d3a53157b3e17))
* Made Odin show generic classes correctly in the UnityEvent UI. ([b5ce462](https://github.com/SolidAlloy/GenericUnityObjects/commit/b5ce462f510db3147e6b9df28bda5936b6d223d8))

# [2.2.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.1.0...2.2.0) (2021-02-06)


### Bug Fixes

* Added back using statements that are required with GENERIC_UNITY_OBJECTS_DEBUG ([4c287e5](https://github.com/SolidAlloy/GenericUnityObjects/commit/4c287e544e0f775ee04bfd0caa92868ba4418431))
* Allowed label to be null for generic object field ([cb10ff3](https://github.com/SolidAlloy/GenericUnityObjects/commit/cb10ff3c83492967058bd0e354a130a7cbdf559b))
* Fixed the missing dependency caused by SolidUtilities update ([7aced0c](https://github.com/SolidAlloy/GenericUnityObjects/commit/7aced0c644518c3250d5fb9523847795847ce75a))


### Features

* Started adding components to the component menu by their full namespace path ([9133ccf](https://github.com/SolidAlloy/GenericUnityObjects/commit/9133ccfbb71313fd66a523b9793a50b8168ffe4b))

# [2.1.0](https://github.com/SolidAlloy/GenericUnityObjects/compare/2.0.1...2.1.0) (2021-01-30)


### Features

* Moved GenericUnityObjects folder into Plugins ([9cda042](https://github.com/SolidAlloy/GenericUnityObjects/commit/9cda04242bb04392e30a2fa128030f387ad44bb9))

# [1.2.0](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.5...1.2.0) (2021-01-30)


### Features

* Allowed usage of non-serialized types when creating generic UnityEngine Objects and removed the possibility to choose custom namespace name and scripts path when generating concrete types from generic scriptable objects ([b66a7e5](https://github.com/SolidAlloy/GenericScriptableObjects/commit/b66a7e5d46c869d2fc49cc624bab8a3cae14d830))
* Removed GenericScriptableObject.CreateInstance(Type genericTypeWithoutTypeParams, params Type[] paramTypes) ([a4b7505](https://github.com/SolidAlloy/GenericScriptableObjects/commit/a4b750591d582770a393c5fc8c8cd5f38429081c))


### Performance Improvements

* Improved performance in GenericTypesChecker and DictInitializer ([6206842](https://github.com/SolidAlloy/GenericScriptableObjects/commit/6206842e562ad28fcaaa9cb8c6a23cbc1fab77e7))
* Significantly improved performance in ArgumentsChecker ([545b8b5](https://github.com/SolidAlloy/GenericScriptableObjects/commit/545b8b5069d933abf9fc5d1ca5940e8c34bfc665))

## [1.1.4](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.3...1.1.4) (2020-12-03)


### Bug Fixes

* Default file name for a generic SO is now using short type name instead of full one. ([7b690b3](https://github.com/SolidAlloy/GenericScriptableObjects/commit/7b690b35ed70cc54e7da602036ceb49a8e286da6))
* Disable the warning that appeared after the usage sample is imported ([0d0a24b](https://github.com/SolidAlloy/GenericScriptableObjects/commit/0d0a24ba27d2ef1b02fe24e21ab6e82cd9fbb146))
* Fixed asset creation menus not appearing immediately after the samples import. ([15a8baa](https://github.com/SolidAlloy/GenericScriptableObjects/commit/15a8baa459cdb9201dda2aa0aae3e97a03640530))
* Fixed the bug where methods were added to the MenuItems class on editor start and caused compilation errors ([f44a920](https://github.com/SolidAlloy/GenericScriptableObjects/commit/f44a920abdb6f14ad1985d8731296ecaa5c376b6))
* Reduced GenericSODrawer.OnGUI() execution time by using a type cache ([5adf695](https://github.com/SolidAlloy/GenericScriptableObjects/commit/5adf695af67273d54ad6fdbe271c06a0f09823a7))
* Replaced a method available only in Unity 2020.1+ ([ca415b6](https://github.com/SolidAlloy/GenericScriptableObjects/commit/ca415b61d4478ba8c073f24592552d22c00da5d1))
* Started rounding up values of the window position before its creation. ([0a05863](https://github.com/SolidAlloy/GenericScriptableObjects/commit/0a058633b03f28f3e59dc9c2ea96d2c8e784879a))
* Started using the correct name of GenericSOPersistentStorage class ([92cacc2](https://github.com/SolidAlloy/GenericScriptableObjects/commit/92cacc24a9a8eb3234da4437c3f1a934a2b9f924))

## [1.1.3](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.2...1.1.3) (2020-11-09)


### Bug Fixes

* Added an additional error message when generic scriptable object is instantiated in the field initializer ([fc7904e](https://github.com/SolidAlloy/GenericScriptableObjects/commit/fc7904e84f7acc0808dccb2c4db4bd8490599348))
* Marked GenericScriptableObject.CreateInstance() as pure method ([5903a16](https://github.com/SolidAlloy/GenericScriptableObjects/commit/5903a1666a3cb9b63e42b5772f8236bc802f5a78))

## [1.1.2](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.1...1.1.2) (2020-11-08)


### Bug Fixes

* ObjectField of a non-generic class derived from GenericScriptableObject works without errors now ([a7bec7d](https://github.com/SolidAlloy/GenericScriptableObjects/commit/a7bec7d332f651043126206e1858ce38da639ec9))

## [1.1.1](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.1.0...1.1.1) (2020-11-05)


### Bug Fixes

* Fixed warning about inconsistent new lines in MenuItems.cs ([44e5231](https://github.com/SolidAlloy/GenericScriptableObjects/commit/44e5231d5b3dd0e980641db24e37744437433a01))

# [1.1.0](https://github.com/SolidAlloy/GenericScriptableObjects/compare/1.0.0...1.1.0) (2020-11-04)


### Features

* Added a helper that removes auto-generated code after the GenericScriptableObject class removal ([83d6b41](https://github.com/SolidAlloy/GenericScriptableObjects/commit/83d6b419a13d33bb34b12a599b50179983329384))

# 1.0.0 (2020-11-03)


### Bug Fixes

* The database is now saved to disk when changed ([2787414](https://github.com/SolidAlloy/GenericScriptableObjects/commit/27874147f4ab49e87d497fbb5fb65df12d5e14e3))
