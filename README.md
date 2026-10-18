# ET Plugin Packages

All **ET Plugin** packages can be installed either through the **ET Plugin Scoped Registry** (recommended — shows all packages directly inside Unity Package Manager's **My Registries**, just like OpenUPM) or via direct Git URLs.

---

## Option 1: Install via Scoped Registry (Recommended — Like OpenUPM)

### Step 1: Add the Scoped Registry to `Packages/manifest.json`

Open your Unity project's `Packages/manifest.json` (or go to **Edit -> Project Settings -> Package Manager -> Scoped Registries**) and add:

```json
{
  "scopedRegistries": [
    {
      "name": "ET Plugin Registry",
      "url": "https://tonicatfelidae.github.io/ETPlugin",
      "scopes": [
        "com.nekoniiistudio.et"
      ]
    }
  ]
}
```

### Step 2: Browse & Install in Unity Package Manager

1. Open **Window -> Package Manager** in the Unity Editor.
2. Change the top-left dropdown from **Packages: In Project** to **Packages: My Registries**.
3. All `com.nekoniiistudio.et.*` packages appear under **Nekoniii Studio** ready for one-click **Install** or **Update**!

---

## Available Packages

| Display Name | Package ID | Latest Version | Description |
|--------------|------------|----------------|-------------|
| **ET Support Kit** | `com.nekoniiistudio.et.supportkit` | `1.29.5` | Core foundation utilities and data structures |
| **ET UI Kit** | `com.nekoniiistudio.et.uikit` | `1.1.6` | UI utilities and scrollview components |
| **ET Resource Loader** | `com.nekoniiistudio.et.resourceloader` | `1.0.0` | Resource & Addressables loading utilities |
| **ET SaveLoad** | `com.nekoniiistudio.et.saveload` | `1.1.1` | Save & Load persistence framework |
| **ET Version Control** | `com.nekoniiistudio.et.versioncontrol` | `1.0.0` | Version tracking utilities |
| **ET Input Manager** | `com.nekoniiistudio.et.inputmanager` | `1.0.0` | Runtime input management |
| **ET Engine** | `com.nekoniiistudio.et.engine` | `1.0.5` | Legacy ET Engine runtime package |
| **ET Dialogue** | `com.nekoniiistudio.et.dialogue` | `1.0.5` | Runtime dialogue system |
| **ET Dev Tools** | `com.nekoniiistudio.et.devtools` | `1.0.0` | Runtime development & debugging tools |
| **ET Mobile Kit** | `com.nekoniiistudio.et.mobilekit` | `1.0.1` | Mobile utilities (notifications, rating, etc.) |
| **ET Monetization AppLovin** | `com.nekoniiistudio.et.monetization.applovin` | `1.0.0` | AppLovin MAX mediation & attribution adapter |
| **ET Monetization Google Mobile Ads** | `com.nekoniiistudio.et.monetization.googleads` | `1.0.0` | Google Mobile Ads (AdMob) adapter & safe area |
| **ET Monetization In-App Purchasing** | `com.nekoniiistudio.et.monetization.iap` | `1.0.0` | Unity IAP v5+ manager, bundle ownership & `ETIAPButton` |

---

## Option 2: Direct Git URLs

If you prefer installing directly via Git URL in **Package Manager -> + -> Add package from git URL...**:

- **ET SupportKit**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.supportkit#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.supportkit#main)
- **ET uiKit**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.uikit#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.uikit#main)
- **ET ResourceLoader**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.resourceloader#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.resourceloader#main)
- **ET SaveLoad**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.saveload#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.saveload#main)
- **ET Version Control**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.versioncontrol#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.versioncontrol#main)
- **ET Input Manager**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.inputmanager#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.inputmanager#main)
- **ET Engine**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.engine#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.engine#main)
- **ET Dialogue**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.dialogue#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.dialogue#main)
- **ET DevTools**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.devtools#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.devtools#main)
- **ET Mobilekit**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.mobilekit#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.mobilekit#main)
- **ET Monetization AppLovin**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.applovin#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.applovin#main)
- **ET Monetization Google Mobile Ads**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.googleads#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.googleads#main)
- **ET Monetization IAP**  
  [`https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.iap#main`](https://github.com/TonicatFelidae/ETPlugin.git?path=Packages/com.nekoniiistudio.et.monetization.iap#main)

---

## Publishing / Updating Packages in the Scoped Registry

- **Automatic (GitHub Actions)**: Whenever you bump `"version"` in any `Packages/com.nekoniiistudio.et.*/package.json` and push to `main`, `.github/workflows/publish-upm-registry.yml` automatically packs the `.tgz` tarballs, updates `/-/v1/search` and `/-/all`, preserves previous versions, and publishes to the `gh-pages` branch.
- **Manual / Local Build**:
  ```bash
  python3 .github/scripts/build_upm_registry.py \
    --packages-root Packages \
    --output-dir _dist_registry \
    --registry-url "https://tonicatfelidae.github.io/ETPlugin"
  ```
