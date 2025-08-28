# Metallic Packer

**Metallic Packer** is a Unity Editor extension that generates a Metallic Map suitable for Unity's Standard/URP/HDRP workflows.  
It combines **Metalness (R)** and **Smoothness (A)** into a single texture.  

- R = Metalness  
- A = Smoothness  
- G, B = unused (0)  

---

## ✨ Features
- **Metalness input (optional)**  
  If omitted, R=0 (non-metal).  
- **Roughness/Smoothness input (required)**  
  - If the input is Roughness → converted to Smoothness (A = 1 - value)  
  - If the input is already Smoothness → used directly (passthrough)  
- **Source Channel selectable** (R, G, B, A) for the Roughness/Smoothness texture.  
- Works with any readable/unreadable texture (GPU-based sampling).  

---

## 📥 Installation

### Option 1. Git URL (UPM)
In Unity, open **Window > Package Manager > Add package from git URL...**  
and paste:

```
https://github.com/hijiki96/metallic-packer.git#v0.1.0
```

### Option 2. `manifest.json`
Alternatively, edit `Packages/manifest.json`:

```json
"dependencies": {
  "com.hijiki-tools.metallic-packer": "https://github.com/hijiki96/metallic-packer.git#v0.1.0"
}
```

---

## 🔧 Usage

1. Open the tool from Unity menu:  
   **Tools > Textures > Metallic Packer (Single)**

2. Assign textures:
   - **Metalness (optional)** → assign a grayscale metalness map (white=metal, black=non-metal). Leave empty for R=0.  
   - **Roughness / Smoothness (required)** → assign a roughness or smoothness map.  

3. Configure options:
   - **Source Channel** → select which channel to read from.  
   - **Input is Roughness** → check if the texture is roughness (inverted to smoothness).  

4. Press **Pack Selected**.  
   → A PNG is saved with **R=Metalness, A=Smoothness**.  

---

## 📂 Output

- Output file is saved as `Metallic.png` (or chosen name).  
- Import settings:  
  - Color Space = Linear (sRGB OFF)  
  - Alpha = Smoothness (Alpha-is-Transparency OFF)  

---

## 📝 License
MIT © 2025 [hijiki96](https://github.com/hijiki96)
