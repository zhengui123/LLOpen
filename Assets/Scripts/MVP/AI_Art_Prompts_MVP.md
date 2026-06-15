# 《榴莲开了 MVP》- AI绘图提示词文档

> 版本：MVP v1.0 | 日期：2026-06-15
> 对比完整版：110张 → MVP仅25张（核心贴图，其余用色块Placeholder）
> 视觉风格：2D卡通/手绘风格，色彩鲜明，食物插画风

---

## 画风统一定义

**基础画风关键词**（所有提示词共用）：
```
2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, warm lighting, appetizing colors, flat shading with soft gradients, no outlines, mobile game UI aesthetic
```

**通用 Negative Prompt**：
```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
```

---

## MVP贴图清单总览

| 编号范围 | 类别 | 数量 | 尺寸 | 格式 | 优先级 |
|---------|------|------|------|------|--------|
| S-01~S-02 | 场景背景 | 2张 | 1080×1920px | JPG | P0 |
| D-01~D-03 | 榴莲未开（普通外表）| 3张 | 512×512px | PNG | P0 |
| D-A01~D-A09 | 榴莲未开（外观变体）| 9张 | 512×512px | PNG | P0 |
| D-O01~D-O06 | 榴莲已开（出肉率变体）| 6张 | 512×512px | PNG | P0 |
| U-01~U-04 | 外观等级图标 | 4张 | 256×256px | PNG | P1 |
| F-01 | 市场3选1框架 | 1张 | 1080×1920px | PNG | P1 |
| **合计** | | **25张** | | | |

> **MVP原则**：场景背景只做2张（市场+开果台），榴莲只做3品种，出肉率只做2种极端（空壳+爆肉），中间档次用色块Placeholder。外观变体只做劣质/优质/极品（普通外表=D-01~D-03）。

---

## 一、场景背景图（2张）

---

### S-01 市场背景

- **用途**：榴莲市场主界面，玩家选购榴莲
- **尺寸**：1080×1920px
- **格式**：JPG
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, cozy atmosphere, warm ambient lighting, detailed background illustration, a bustling Asian durian wholesale market at night, multiple fruit stalls with colorful awnings and string lights, wooden crates filled with durians, hanging lanterns casting warm orange glow, wet ground reflecting lights, market vendors behind stalls, tropical fruit decorations, neon signs in Chinese, steam rising from food vendors, lively market atmosphere, no characters in foreground, vertical composition for mobile screen, warm color palette with yellows oranges and greens
  ```

---

### S-02 开果台背景

- **用途**：开榴莲交互界面背景
- **尺寸**：1080×1920px
- **格式**：JPG
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, cozy atmosphere, warm lighting, close-up top-down view of a wooden cutting board work surface for opening durians, clean wooden table texture, a large knife placed on the side, small bowl for durian flesh, scattered durian thorn fragments, warm overhead spotlight creating soft shadows, clean organized workspace, subtle wood grain texture, warm amber lighting, intimate focused atmosphere, vertical composition
  ```

---

## 二、榴莲未开状态（12张）

### 2.1 普通外表（3张）—— D-01~D-03

---

### D-01 金枕榴莲 - 普通外表（未开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, pale yellowish green shell with short dense blunt thorns, normal healthy appearance, fresh green leaf on top, oval shape, no cracks or special features, clean standard durian look, soft natural lighting, soft shadow, isolated on white background
  ```

---

### D-02 干尧榴莲 - 普通外表（未开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, medium green shell with medium-length sharp thorns, normal healthy appearance, fresh green leaf on top, slightly elongated oval shape, clean standard durian look, soft natural lighting, soft shadow, isolated on white background
  ```

---

### D-03 猫山王榴莲 - 普通外表（未开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, rich deep green shell with long sharp thorns, normal healthy appearance, fresh green leaf on top, round plump shape, premium durian appearance, soft natural lighting, soft shadow, isolated on white background
  ```

---

### 2.2 外观变体（9张）—— D-A01~D-A09

> 每个品种有3种外观变体（劣质/优质/极品），普通外表已完成（D-01~D-03）。

---

### D-A01 金枕 - 劣质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, POOR APPEARANCE: visible cracks on the shell surface, dull muted pale yellowish green color, wilted brown dried leaf on top, dark spots on shell, uninspiring disappointing look, slightly squashed shape, soft shadow, isolated on white background
  ```

---

### D-A02 金枕 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, GOOD APPEARANCE: bright vibrant pale yellowish green shell, fresh green healthy leaf, subtle golden sheen on shell surface, no cracks, perfect round shape, promising visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

---

### D-A03 金枕 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, PREMIUM APPEARANCE: intense golden glow aura around the fruit, shell is perfect golden yellow color, leaf is vivid emerald green with dewdrop sparkles, floating golden sparkle particles, divine legendary quality appearance, flawless shell surface, majestic presentation, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

---

### D-A04 干尧 - 劣质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, POOR APPEARANCE: visible cracks on the shell, dull muted medium green color, wilted brown dried leaf, dark spots on shell, disappointing look, slightly elongated squashed shape, soft shadow, isolated on white background
  ```

---

### D-A05 干尧 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, GOOD APPEARANCE: bright vibrant medium green shell, fresh green healthy leaf, subtle golden sheen, no cracks, perfect elongated oval shape, promising visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

---

### D-A06 干尧 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, PREMIUM APPEARANCE: intense golden glow aura, perfect deep green shell with golden sheen, vivid emerald leaf with dewdrop sparkles, floating golden sparkle particles, flawless surface, majestic presentation, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

---

### D-A07 猫山王 - 劣质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, POOR APPEARANCE: visible cracks on the shell, dull muted dark green color, wilted brown dried leaf, dark spots on shell, disappointing look, slightly squashed round shape, soft shadow, isolated on white background
  ```

---

### D-A08 猫山王 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, GOOD APPEARANCE: bright vibrant deep green shell, fresh green healthy leaf, subtle golden sheen, no cracks, perfect plump round shape, premium quality visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

---

### D-A09 猫山王 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, PREMIUM APPEARANCE: intense golden glow aura, perfect deep forest green shell with golden sheen, vivid emerald leaf with dewdrop sparkles, floating golden sparkle particles, flawless surface, legendary king quality appearance, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

---

## 三、榴莲已开状态（6张）—— D-O01~D-O06

> MVP只做2种极端出肉率（空壳+爆肉），中间档位用色块Placeholder。

---

### D-O01 金枕 - 空壳（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Jinzheng variety, EMPTY SHELL result: durian shell cracked open showing completely empty interior, no fruit flesh visible anywhere, just hollow brown shell interior, disappointing empty result, sad comedic "nothing inside" visual, soft shadow, isolated on white background
  ```

---

### D-O02 金枕 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Jinzheng variety, PERFECT FULL FLESH result: durian shell cracked open revealing 5 compartments all completely filled with plump golden yellow durian flesh, overflowing abundance, rich appetizing golden color, glossy moist texture, exciting amazing visual of maximum yield, dramatic warm spotlight, soft shadow, isolated on white background
  ```

---

### D-O03 干尧 - 空壳（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Ganyao variety, EMPTY SHELL result: durian shell cracked open showing completely empty interior, no fruit flesh visible, just hollow interior, disappointing empty result, soft shadow, isolated on white background
  ```

---

### D-O04 干尧 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Ganyao variety, PERFECT FULL FLESH result: durian shell cracked open revealing all compartments completely filled with plump golden yellow durian flesh, overflowing abundance, rich appetizing color, glossy moist texture, exciting visual, soft shadow, isolated on white background
  ```

---

### D-O05 猫山王 - 空壳（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Musang King variety, EMPTY SHELL result: durian shell cracked open showing completely empty interior, no fruit flesh visible, just hollow interior, disappointing empty result for premium variety, soft shadow, isolated on white background
  ```

---

### D-O06 猫山王 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Musang King variety, PERFECT FULL FLESH result: durian shell cracked open revealing 7 compartments all completely filled with plump creamy golden yellow durian flesh, overflowing abundance, richest appetizing color, glossy moist luxurious texture, ultimate exciting visual of maximum yield from king variety, dramatic warm spotlight, soft shadow, isolated on white background
  ```

---

## 四、UI图标（5张）

---

### U-01 劣质外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing poor durian appearance, cracked egg-like shape in dull brown-green color, sad looking icon, rating 1 star implied, game UI icon for "poor quality"
  ```

---

### U-02 普通外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing normal durian appearance, simple durian silhouette in standard green color, neutral looking icon, rating 2-3 stars implied, game UI icon for "normal quality"
  ```

---

### U-03 优质外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing good durian appearance, durian silhouette with subtle golden glow in vibrant green color, happy promising icon, rating 4 stars implied, game UI icon for "good quality"
  ```

---

### U-04 极品外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing premium legendary durian appearance, durian silhouette with intense golden glow aura and sparkle particles, majestic crown-like appearance, rating 5 stars implied, game UI icon for "premium legendary quality"
  ```

---

### F-01 市场3选1背景框架

- **用途**：市场上展示3颗榴莲的卡片框架
- **尺寸**：1080×1920px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, UI design, a decorative frame layout for displaying 3 items in a row for game market selection, 3 equally sized rounded rectangle card slots with spacing between them, warm golden brown wooden shelf texture at bottom, subtle market stall awning decoration at top, fairy lights string across the top, "SELECT YOUR DURIAN" header area, vertical mobile game format, transparent background elements, warm inviting marketplace UI aesthetic
  ```

---

## 五、Placeholder策略（开发用）

MVP阶段，以下贴图使用色块Placeholder，不需要AI生成：

| 占位内容 | Placeholder方案 | 后续版本 |
|---------|----------------|---------|
| 少肉/正常/满肉出肉率 | 灰色/蓝色/绿色半透明色块覆盖在果肉区域上 | v1.1 生成 |
| 按钮UI（购买/卖出/升级）| Unity UI原生Button + 文字 | v1.1 生成图标 |
| 金币图标 | "G"字母圆形色块 | v1.1 生成 |
| 加工路线图标 | MVP无加工系统 | v1.1 |
| 顾客头像 | MVP无顾客系统 | v1.1 |

---

## 六、批量生成执行命令

```bash
cd "C:\Users\1\WorkBuddy\2026-06-11-11-51-10\MVP"
python -u C:\Users\1\.workbuddy\skills\chatgpt-batch-image-gen\batch_gen_gpt_image_2.py ^
  --input AI_Art_Prompts_MVP.md ^
  --output generated_images ^
  --size 1024x1792 ^
  > batch_log_mvp.txt 2>&1
```

**预估**：25张 × 30秒/张 ≈ 12分钟纯生成时间，加排队约15分钟。

---

**文档版本**：MVP v1.0 | 2026-06-15
**下一步**：执行批量生成命令 → 导入Unity → 开始程序开发
