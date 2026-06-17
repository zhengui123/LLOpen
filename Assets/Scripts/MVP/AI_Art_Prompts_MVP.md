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
| K-01, SH-01~02, FL-01~02 | 开果动画贴图 | 5张 | 256-512px | PNG | P0 |
| U-01~U-04 | 外观等级图标 | 4张 | 256×256px | PNG | P1 |
| UI-05~UI-09 | UI图标扩充 | 5张 | 64-256px | PNG | P0 |
| R-01~R-06 | 出肉率评级图标 | 6张 | 256×256px | PNG | P0 |
| F-01 | 市场3选1框架 | 1张 | 1080×1920px | PNG | P1 |
| **合计** | | **41张** | | | |

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

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
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

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
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

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-02 干尧榴莲 - 普通外表（未开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, medium green shell with medium-length sharp thorns, normal healthy appearance, fresh green leaf on top, slightly elongated oval shape, clean standard durian look, soft natural lighting, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-03 猫山王榴莲 - 普通外表（未开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, rich deep green shell with long sharp thorns, normal healthy appearance, fresh green leaf on top, round plump shape, premium durian appearance, soft natural lighting, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
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

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A02 金枕 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, GOOD APPEARANCE: bright vibrant pale yellowish green shell, fresh green healthy leaf, subtle golden sheen on shell surface, no cracks, perfect round shape, promising visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A03 金枕 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Jinzheng variety, PREMIUM APPEARANCE: intense golden glow aura around the fruit, shell is perfect golden yellow color, leaf is vivid emerald green with dewdrop sparkles, floating golden sparkle particles, divine legendary quality appearance, flawless shell surface, majestic presentation, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A04 干尧 - 劣质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, POOR APPEARANCE: visible cracks on the shell, dull muted medium green color, wilted brown dried leaf, dark spots on shell, disappointing look, slightly elongated squashed shape, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A05 干尧 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, GOOD APPEARANCE: bright vibrant medium green shell, fresh green healthy leaf, subtle golden sheen, no cracks, perfect elongated oval shape, promising visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A06 干尧 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Ganyao variety, PREMIUM APPEARANCE: intense golden glow aura, perfect deep green shell with golden sheen, vivid emerald leaf with dewdrop sparkles, floating golden sparkle particles, flawless surface, majestic presentation, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A07 猫山王 - 劣质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, POOR APPEARANCE: visible cracks on the shell, dull muted dark green color, wilted brown dried leaf, dark spots on shell, disappointing look, slightly squashed round shape, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A08 猫山王 - 优质外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, GOOD APPEARANCE: bright vibrant deep green shell, fresh green healthy leaf, subtle golden sheen, no cracks, perfect plump round shape, premium quality visual cue, warm natural lighting, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-A09 猫山王 - 极品外表

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a whole durian fruit, Musang King variety, PREMIUM APPEARANCE: intense golden glow aura, perfect deep forest green shell with golden sheen, vivid emerald leaf with dewdrop sparkles, floating golden sparkle particles, flawless surface, legendary king quality appearance, dramatic warm golden lighting from above, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
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

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-O02 金枕 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Jinzheng variety, PERFECT FULL FLESH result: durian shell cracked open revealing 5 compartments all completely filled with plump golden yellow durian flesh, overflowing abundance, rich appetizing golden color, glossy moist texture, exciting amazing visual of maximum yield, dramatic warm spotlight, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-O03 干尧 - 空壳（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Ganyao variety, EMPTY SHELL result: durian shell cracked open showing completely empty interior, no fruit flesh visible, just hollow interior, disappointing empty result, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-O04 干尧 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Ganyao variety, PERFECT FULL FLESH result: durian shell cracked open revealing all compartments completely filled with plump golden yellow durian flesh, overflowing abundance, rich appetizing color, glossy moist texture, exciting visual, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-O05 猫山王 - 空壳（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Musang King variety, EMPTY SHELL result: durian shell cracked open showing completely empty interior, no fruit flesh visible, just hollow interior, disappointing empty result for premium variety, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### D-O06 猫山王 - 爆肉（已开）

- **尺寸**：512×512px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a durian fruit split open, Musang King variety, PERFECT FULL FLESH result: durian shell cracked open revealing 7 compartments all completely filled with plump creamy golden yellow durian flesh, overflowing abundance, richest appetizing color, glossy moist luxurious texture, ultimate exciting visual of maximum yield from king variety, dramatic warm spotlight, soft shadow, isolated on white background
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---


## 四、开果动画贴图（5张）—— K-01, SH-01~SH-02, FL-01~FL-02

> 这些贴图是 Phase 5 开果动画的核心素材。没有它们，划刀和壳裂动画无法实现。

---

### K-01 水果刀

- **用途**：Phase 5.1 KnifeTool，手指拖动划壳的手持刀贴图。独立于背景中装饰用的刀，这是玩家操作的交互道具
- **尺寸**：256×512px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a single handheld fruit knife for opening durians, sharp curved stainless steel blade with wooden handle, slightly curved blade shape for fruit opening, clean shiny blade with subtle reflection, warm brown wood handle with visible grain, top-down view angle, isolated on white background, no hand holding it, knife only, simple and clear silhouette for game UI, soft shadow below
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, hand holding knife, blood, weapon, combat knife, chef knife too large, cleaver
  ```

---

### SH-01 榴莲壳 - 左半

- **用途**：Phase 5.2 DurianOpener，壳裂开动画的左侧半壳，向左侧分离移动
- **尺寸**：512×512px
- **格式**：PNG（透明背景）
- **说明**：这是从中间劈开的左半边榴莲壳，内侧朝外。用通用榴莲壳色（黄绿色），不区分品种（因为玩家最关心的是里面的肉，壳只是过渡动画素材）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, the LEFT HALF of a durian shell that has been split in half from the top, showing the hollow interior of the shell, pale yellowish green shell exterior with blunt thorns on the outside, pale cream colored inner shell surface visible, split edge is clean and straight (cut by knife), half-round bowl shape, top-down angle slightly tilted, isolated on transparent background, simple silhouette for animation sprite
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, durian flesh inside, whole durian, right half, people
  ```

---

### SH-02 榴莲壳 - 右半

- **用途**：Phase 5.2 DurianOpener，壳裂开动画的右侧半壳，向右侧分离移动
- **尺寸**：512×512px
- **格式**：PNG（透明背景）
- **说明**：右半边榴莲壳，与 SH-01 配对使用。同样不区分品种，只做一张通用壳
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, the RIGHT HALF of a durian shell that has been split in half from the top, showing the hollow interior of the shell, pale yellowish green shell exterior with blunt thorns on the outside, pale cream colored inner shell surface visible, split edge is clean and straight (cut by knife), half-round bowl shape, top-down angle slightly tilted, isolated on transparent background, simple silhouette for animation sprite
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, durian flesh inside, whole durian, left half, people
  ```

---

### FL-01 单房果肉块

- **用途**：Phase 5.2 DurianOpener，逐房揭示时"有肉"的房显示此贴图。一个金黄色饱满的榴莲果肉块
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a single plump pillow-shaped piece of golden yellow durian flesh, rich appetizing warm golden color, soft glossy moist texture, creamy appearance, slightly rounded pillow shape with gentle curves, one individual fruit segment that would fit in a durian compartment, isolated on transparent background, delicious premium fruit appearance, soft warm lighting
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, whole durian, shell, plate, bowl, multiple pieces
  ```

---

### FL-02 单房空壳块

- **用途**：Phase 5.2 DurianOpener，逐房揭示时"空房"的房显示此贴图。一个灰色干瘪的空壳房块
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, food illustration style, a single empty dried-out durian compartment pod, dull gray-brown color, shriveled and withered appearance, thin dry texture, showing the shell interior but completely devoid of fruit flesh, disappointing empty result, slightly concave shape, isolated on transparent background, sad comedic "empty" visual cue
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, golden flesh, yellow fruit, whole durian, multiple pieces
  ```

---

## 五、UI图标（5张）

---

### U-01 劣质外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing poor durian appearance, cracked egg-like shape in dull brown-green color, sad looking icon, rating 1 star implied, game UI icon for "poor quality"
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### U-02 普通外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing normal durian appearance, simple durian silhouette in standard green color, neutral looking icon, rating 2-3 stars implied, game UI icon for "normal quality"
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### U-03 优质外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing good durian appearance, durian silhouette with subtle golden glow in vibrant green color, happy promising icon, rating 4 stars implied, game UI icon for "good quality"
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### U-04 极品外表图标

- **尺寸**：256×256px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon representing premium legendary durian appearance, durian silhouette with intense golden glow aura and sparkle particles, majestic crown-like appearance, rating 5 stars implied, game UI icon for "premium legendary quality"
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---

### F-01 市场3选1背景框架

- **用途**：市场上展示3颗榴莲的卡片框架
- **尺寸**：1080×1920px | **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, UI design, a decorative frame layout for displaying 3 items in a row for game market selection, 3 equally sized rounded rectangle card slots with spacing between them, warm golden brown wooden shelf texture at bottom, subtle market stall awning decoration at top, fairy lights string across the top, "SELECT YOUR DURIAN" header area, vertical mobile game format, transparent background elements, warm inviting marketplace UI aesthetic
  ```

- **Negative Prompt**：
  ```
low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft
  ```


---



---


## 六、UI图标扩充（5张）—— UI-05~UI-09

> 补充 Phase 6 各页面必需的UI元素贴图。

---

### UI-05 金币图标

- **用途**：Phase 6.1/6.3/6.5：顶部余额显示、卖出金币增加动画、商店升级费用显示
- **尺寸**：128×128px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a shiny gold coin icon for game currency display, circular gold coin with bright golden yellow color, subtle radial shine lines from center, small dollar or yuan symbol embossed in center, rich metallic gold gradient, game UI coin icon, clean edges, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, pile of coins, multiple coins, bitcoin, crypto, paper money
  ```

---

### UI-06 看广告图标

- **用途**：Phase Ads，所有"看广告获取奖励"按钮上的统一图标（免费试闻、加价、复活等按钮共用）
- **尺寸**：128×128px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small icon for "watch ad to get reward" button, stylized play button triangle combined with a gift box or star sparkle, bright eye-catching colors (orange and gold), small film strip or video camera element, friendly inviting look, game UI button icon, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, youtube logo, ad text, price tag
  ```

---

### UI-07 滑动手势引导

- **用途**：Phase 6.2 OpenPage，在榴莲顶部显示的手势指引动画关键帧，"在榴莲顶部滑动开果"
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, UI tutorial element, a hand gesture guide icon showing a finger swiping motion, simplified cartoon hand with index finger extended pointing and making a swipe motion trail, white or light gray semi-transparent hand, motion trail lines showing the swipe direction from left to right (horizontal swipe), gentle glow around the finger tip, tutorial hint visual, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, whole arm, detailed hand anatomy, realistic finger
  ```

---

### UI-08 返回箭头按钮

- **用途**：Phase 6.1~6.5，各页面左上角的返回/退出按钮
- **尺寸**：64×64px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, flat design, simple shapes, clear silhouette, a small back arrow button icon for game UI navigation, left-pointing arrow shape in warm brown or gold color, simple clean arrow silhouette with slightly rounded corners, circular button background in semi-transparent dark color, game UI navigation element, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, right arrow, double arrow, thick heavy design
  ```

---

### UI-09 品种选择按钮底图

- **用途**：Phase 6.1 MarketPage，金枕/干尧/猫山王三个品种选择按钮的通用底图
- **尺寸**：256×64px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, UI element, a horizontal rounded rectangle button background for variety selection in a fruit market game, warm wooden brown color with subtle wood grain texture, rounded corners, soft inner shadow for depth, slightly raised 3D button appearance, clean and inviting design, game UI button template, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, text on button, specific fruit image, too ornate
  ```

---

## 七、出肉率评级图标（6张）—— R-01~R-06

> Phase 6.2 OpenPage，开果完成后根据出肉率显示对应评级图标。
> 这些是重要的情绪反馈元素，用于强化"开盲盒"的快感。

---

### R-01 空壳评级图标

- **用途**：出肉率 0-15% 的评级标识，视觉反馈：灰色 + 碎裂
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, EMPTY SHELL result, gray cracked egg or shell shape, sad disappointed visual tone, dull gray and dark colors, small crack lines radiating from center, "worst result" visual language, game UI rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, smile, happy, gold, bright colors, stars
  ```

---

### R-02 小亏评级图标

- **用途**：出肉率 15-30% 的评级标识，视觉反馈：黄色 + 苦笑表情
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, SLIGHT LOSS result, yellow circle badge, wry smile emoji-like face with sweat drop, "could be worse" visual tone, warm yellow and light orange colors, slightly disappointed but not terrible mood, game UI rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, crying, angry, gold, stars, celebration
  ```

---

### R-03 回本评级图标

- **用途**：出肉率 30-45% 的评级标识，视觉反馈：蓝色 + 平淡表情
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, BREAK EVEN result, blue circle badge, neutral straight-line mouth emoji-like face, "not bad not great" visual tone, calm blue and light cyan colors, balanced neutral mood, game UI rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, sad, happy, gold, stars, celebration
  ```

---

### R-04 小赚评级图标

- **用途**：出肉率 45-60% 的评级标识，视觉反馈：绿色 + 微笑
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, SMALL PROFIT result, green circle badge, happy smiling emoji-like face with closed happy eyes, "nice win" visual tone, bright green and lime colors, cheerful positive mood, small sparkle accents on sides, game UI rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, sad, crying, angry, over the top celebration
  ```

---

### R-05 大赚评级图标

- **用途**：出肉率 60-75% 的评级标识，视觉反馈：金色 + 大笑
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, BIG PROFIT result, golden circle badge with radial shine, laughing emoji-like face with open mouth and joy tears, "jackpot" visual tone, rich gold and warm yellow colors, exciting happy mood, star sparkles and coins floating around the badge, premium celebration feeling, game UI rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, sad, crying, angry, rainbow (save for durian king)
  ```

---

### R-06 榴莲之王评级图标

- **用途**：出肉率 75%+ 的评级标识，视觉反馈：彩虹 + 全屏特效，这是游戏中最稀有的结果
- **尺寸**：256×256px
- **格式**：PNG（透明背景）
- **Prompt**：
  ```
  2D casual mobile game art, cartoon style, vibrant colors, clean lines, icon design, a result rating badge for durian opening game, DURIAN KING legendary result, circular badge with rainbow gradient border and golden crown on top, ecstatic emoji-like face with star eyes and wide open laughing mouth, rainbow color burst radiating outward, golden sparkles and diamond particles floating around, "absolute legend jackpot" visual tone, most premium celebration feeling, tiny durian silhouette inside a royal crest design, game UI ultimate rating badge, isolated on transparent background
  ```
- **Negative Prompt**：
  ```
  low quality, blurry, pixelated, distorted, deformed, ugly, realistic, photographic, 3D render, dark, gloomy, scary, horror, text, watermark, logo, signature, extra limbs, bad anatomy, noisy, grainy, overexposed, underexposed, messy lines, sketch, rough draft, sad, crying, plain, simple, boring
  ```


## 八、Placeholder策略（开发用）

MVP阶段，以下贴图使用色块Placeholder，不需要AI生成：

| 占位内容 | Placeholder方案 | 后续版本 |
|---------|----------------|---------|
| 少肉/正常/满肉出肉率 | 灰色/蓝色/绿色半透明色块覆盖在果肉区域上 | v1.1 生成 |
| 按钮UI（购买/卖出/升级）| Unity UI原生Button + 文字 | v1.1 生成图标 |
| 金币图标 | "G"字母圆形色块 | v1.1 生成 |
| 加工路线图标 | MVP无加工系统 | v1.1 |
| 顾客头像 | MVP无顾客系统 | v1.1 |

---

## 九、批量生成执行命令

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
