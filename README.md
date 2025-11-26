# Boutique Diayma

## Réponses aux questions et guide de débogage (Rider)


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

