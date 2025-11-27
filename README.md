# Boutique Diayma

## Réponses aux questions et guide de débogage (Rider)

### 1. Récupérez dans Visual Studio et exécutez le code fourni lien Github.

**Récupération du projet**

- Cloner le projet depuis le dépôt GitHub fourni
- Ouvrir la solution dans Rider

### 2. Quels sont les projets de la solution ?

1 projet: « Diayma »
Fichier: P2FixAnAppDotNetCode\Diayma.csproj
Référence dans la solution: Diayma.sln → Project = "Diayma".

### 3. Quelle est la version SDK .NET utilisée par ces projets ?

SDK: Microsoft.NET.Sdk.Web (déclaré dans le .csproj)
Target Framework: netcoreapp2.0 (ASP.NET Core 2.0)
Packages principaux:
Microsoft.AspNetCore.All 2.0.6
Microsoft.Extensions.Localization 2.1.1 (note : version supérieure au reste de la pile 2.0.x)
Outil CLI:
Microsoft.VisualStudio.Web.CodeGeneration.Tools 2.0.3

### 4. Installez le SDK

Sur Rider après avoir cloner le repo, en exécutant le projet on nous demande a télécharger ASP.NET Core IIS Module
on va sur ce lien : https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-10.0.100-windows-x64-installer

et on télécharge le SDK

![[Pasted image 20251126220741.png]]

et on l'installe dans le pc
### 5. Créez votre propre dépôt GitHub pour y stocker le code

![[Pasted image 20251126222227.png]]
![[Pasted image 20251126222551.png]]
![[Pasted image 20251126222643.png]]

### 6. Explorez l’application. Signalez 2 bugs trouvés ?

i)
Calcul du total du panier ignore la quantité
Fichier: P2FixAnAppDotNetCode\Models\Cart.cs
Méthode: GetTotalValue()
Ligne actuelle: return GetCartLineList().Sum(x => x.Product.Price);
Problème: le total ne multiplie pas par Quantity, ce qui sous-estime le montant.
Correction suggérée: return GetCartLineList().Sum(x => x.Product.Price * x.Quantity);

ii)
Risque de NullReferenceException lors de la recherche de produit
Fichier: P2FixAnAppDotNetCode\Models\Cart.cs
Méthode: FindProductInCartLines(int productId)
Ligne actuelle: return GetCartLineList().Where(x => x.Product.Id == productId).FirstOrDefault().Product;
Problème: FirstOrDefault() peut renvoyer null; l’accès à .Product provoquerait une exception si le produit n’est pas trouvé.
Corrections possibles:
return GetCartLineList().FirstOrDefault(x => x.Product.Id == productId)?.Product;
ou tester le résultat et retourner null/lever une exception contrôlée.

## 7) Où je place les points d’arrêt (question 7)

Voici précisément où je place mes points d’arrêt. J’ouvre chaque fichier et je clique dans la marge à gauche, sur la ligne indiquée.

a) CartSummaryViewComponent ligne 12
- Fichier: `P2FixAnAppDotNetCode\Components\CartSummaryViewComponent.cs`
- Ligne 12: `_cart = cart as Cart;`

b) ProductController ligne 15
- Fichier: `P2FixAnAppDotNetCode\Controllers\ProductController.cs`
- Ligne 15: `_productService = productService;` (dans le constructeur)

c) OrderController ligne 17
- Fichier: `P2FixAnAppDotNetCode\Controllers\OrderController.cs`
- Ligne 17: `_cart = pCart;` (dans le constructeur)

d) CartController ligne 15
- Fichier: `P2FixAnAppDotNetCode\Controllers\CartController.cs`
- Ligne 15: `_cart = pCart;` (dans le constructeur)

e) Startup ligne 20
- Fichier: `P2FixAnAppDotNetCode\Startup.cs`
- Ligne 20: `Configuration = configuration;` (dans le constructeur)

Remarque: si les numéros de ligne varient un peu dans mon éditeur, je place le point d’arrêt sur l’instruction indiquée.

## 8) Mon guide de débogage Rider (question 8)

### 8.1 Ce que je fais pour lancer le débogage

- J’ouvre la solution `Diayma.sln` dans Rider et je sélectionne la configuration Debug.
- Je clique sur le bouton Debug (icône insecte) pour démarrer l’application en mode débogage.
- Je choisis mon navigateur cible si Rider me le demande.
- L’application démarre; la page par défaut est `Product/Index` (définie via le routage par défaut).

Raccourcis que j’utilise (Windows par défaut, config Rider):
- Pas à pas détaillé (Step Into): F11
- Pas à pas principal (Step Over): F10
- Pas à pas sortant (Step Out): Shift+F11

### 8.2 Comment ça s’exécute jusqu’à l’affichage des produits (chemin typique)

Routes et pipeline (fichiers/références entre parenthèses) que je vois:

1) Démarrage de l’hôte
- Namespace/Classe/Méthode: `P2FixAnAppDotNetCode.Program.Main`
- Appel: `Program.BuildWebHost` → `WebHost.CreateDefaultBuilder(...).UseStartup<Startup>().Build()`

2) Initialisation de Startup
- Namespace/Classe/Méthode: `P2FixAnAppDotNetCode.Startup.Startup(IConfiguration)`
- Mon point d’arrêt « Startup ligne 20 » s’arrête ici.
- Ensuite: `Startup.ConfigureServices(IServiceCollection)` puis `Startup.Configure(IApplicationBuilder, IHostingEnvironment)`
  - Middlewares activés: `UseStaticFiles`, `UseRequestLocalization`, `UseSession`, `UseMvc` (avec route par défaut `{controller=Product}/{action=Index}/{id?}`)

3) Requête HTTP initiale vers la page d’accueil
- Le routeur MVC résout: Controller = `Product`, Action = `Index`.
- Constructeur du contrôleur Produit:
  - Namespace/Classe/Méthode: `P2FixAnAppDotNetCode.Controllers.ProductController.ProductController(IProductService, ILanguageService)`
  - Mon point d’arrêt « ProductController ligne 15 » est atteint (affectation du champ `_productService`).

4) Exécution de l’action qui affiche les produits
- Namespace/Classe/Méthode: `P2FixAnAppDotNetCode.Controllers.ProductController.Index()`
- Code: récupère `List<Product> products = _productService.GetAllProducts();` puis `return View(products);`

5) Rendu de la vue et du layout
- Le layout `Views/Shared/_Layout.cshtml` appelle le composant de vue du panier:
  - `@await Component.InvokeAsync("CartSummary")`
- Cela déclenche l’initialisation et l’invocation du ViewComponent Panier:
  - Constructeur: `P2FixAnAppDotNetCode.Components.CartSummaryViewComponent.CartSummaryViewComponent(ICart)`
    - Mon point d’arrêt « CartSummaryViewComponent ligne 12 » est atteint.
  - Méthode: `P2FixAnAppDotNetCode.Components.CartSummaryViewComponent.Invoke()` → `return View(_cart);`

6) Contrôleurs non sollicités lors de l’affichage initial
- `OrderController` (point d’arrêt ligne 17) et `CartController` (point d’arrêt ligne 15) ne sont pas touchés sur la page d’accueil, car aucune route ou action correspondante n’est appelée durant l’affichage initial des produits. Je les atteins lorsque j’effectue des actions correspondantes (passage commande, gestion du panier, etc.).

### 8.3 Mes conseils de pas à pas
- À l’entrée dans `Program.Main` et `Startup`:
  - J’utilise « Pas à pas principal » (F10) pour survoler le boilerplate, ou « Pas à pas détaillé » (F11) si je veux inspecter `Startup.ConfigureServices` et `Startup.Configure` en profondeur.
- Quand j’arrive au point d’arrêt dans `ProductController` (ligne 15):
  - « Pas à pas principal » (F10) suffit pour avancer jusqu’à `Index()`.
  - J’entre (F11) dans `Index()` pour suivre la récupération des produits.
- Lorsque le layout invoque `CartSummaryViewComponent`:
  - J’utilise « Pas à pas détaillé » (F11) pour entrer dans le constructeur et `Invoke()` si je veux suivre le modèle passé à la vue composant.
- « Pas à pas sortant » (Shift+F11) m’aide à revenir rapidement au pipeline d’appel après inspection d’un détail.

### 8.4 Ce que je vois avant l’affichage des produits (résumé)
- `P2FixAnAppDotNetCode.Program` → `Program.Main` → `Program.BuildWebHost`
- `Microsoft.AspNetCore.WebHost` → `CreateDefaultBuilder` (infrastructure framework)
- `P2FixAnAppDotNetCode.Startup` → `Startup.Startup(IConfiguration)` → `Startup.ConfigureServices` → `Startup.Configure`
- `Microsoft.AspNetCore.Builder` middlewares: `UseStaticFiles`, `UseRequestLocalization`, `UseSession`, `UseMvc`
- `P2FixAnAppDotNetCode.Controllers.ProductController` → constructeur → `Index()`
- `P2FixAnAppDotNetCode.Components.CartSummaryViewComponent` → constructeur → `Invoke()` (via `_Layout.cshtml`)

### 9. Déployez votre solution sous forme d’exécutable Windows.
Pour deployer on va utiliser cette commande :
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

Mais d'abord il faut :
Convertir automatiquement mon projet .NET Core 2.0 → .NET 8  
✔ Reconstruire entièrement Program.cs  
✔ Reconstruire Startup.cs ou le supprimer pour fusionner la config  
✔ Met à jour le `.csproj` (TFM, packages, dépendances obsolètes)  
✔ Rendre compatible _Publish Single File_  
✔ Prépare mon projet pour `win-x64`, `linux-x64`, etc.  
✔ Supprimer les erreurs NETSDK1123 et NETSDK1138

![[Pasted image 20251126230335.png]]
![[Pasted image 20251126230421.png]]
on obtiens un exécutable unique dans :
bin/Release/net8.0/win-x64/publish/
![[Pasted image 20251126231056.png]]
Je vais mettre l'executable dans google drive.
Lien :
pour avoir acces a l'application naviger comme suit : bin/Release/net8.0/win-x64/publish/ et dedans vous verez l'executable 'Diayma'.
Pour tester on va cliquer sur l'executable et voila ce que l'on obtient :
![[Pasted image 20251126233334.png]]
comme on peut ke voir un terminale s'est ouvert puis on nous montre un lien pour acceder a l'application.

Lien google drive : https://drive.google.com/drive/folders/1Oq4lHjC1ogFI5sq7JzpEWVYJkCAkScHN?usp=sharing