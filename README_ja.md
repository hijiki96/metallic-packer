# Metallic Packer

**Metallic Packer** は Unity エディター拡張で、Unity の Standard / URP / HDRP ワークフローに適した Metallic マップを生成するツールです。  
**Metalness (R)** と **Smoothness (A)** を 1 枚のテクスチャにまとめます。  

- R = Metalness  
- A = Smoothness  
- G, B = 未使用（0）  

---

## ✨ 特長
- **Metalness 入力（任意）**  
  指定しない場合は R=0（非金属）として扱われます。  
- **Roughness / Smoothness 入力（必須）**  
  - 入力が Roughness の場合 → Smoothness に変換（A = 1 - 値）  
  - 入力が Smoothness の場合 → そのまま使用  
- **使用するチャンネルを選択可能**（R, G, B, A）  
- 読み取り専用のテクスチャでも動作（GPU経由でサンプリング）  

---

## 📥 インストール方法

### 方法1. Git URL (UPM)
Unity で **Window > Package Manager > Add package from git URL...** を選び、以下を貼り付けてください：

```
https://github.com/hijiki96/metallic-packer.git#v0.1.0
```

### 方法2. `manifest.json` に直接追記
`Packages/manifest.json` を編集し、以下を追加してください：

```json
"dependencies": {
  "com.hijiki-tools.metallic-packer": "https://github.com/hijiki96/metallic-packer.git#v0.1.0"
}
```

---

## 🔧 使い方

1. Unity メニューから開きます：  
   **Tools > Textures > Metallic Packer (Single)**

2. テクスチャを設定します：
   - **Metalness (optional)** → 金属度マップ（白=金属、黒=非金属）。指定しない場合は R=0。  
   - **Roughness / Smoothness (required)** → Roughness または Smoothness マップ。  

3. オプションを設定します：
   - **Source Channel** → 使用するチャンネルを選択。  
   - **Input is Roughness** → チェックすると入力を Roughness として扱い、Smoothness に変換。  

4. **Pack Selected** をクリックします。  
   → R=Metalness、A=Smoothness の PNG が出力されます。  

---

## 📂 出力

- 出力ファイル名は `Metallic.png`（または指定した名前）。  
- インポート設定：  
  - カラースペース = Linear（sRGB OFF）  
  - Alpha = Smoothness（Alpha-is-Transparency OFF）  

---

## 📝 ライセンス
MIT © 2025 [hijiki96](https://github.com/hijiki96)
