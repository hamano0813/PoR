###  苍炎的轨迹 动态修改器

#### 项目说明

用于Dolphin5.0及以上版本，自动适应日文版及汉化版。

#### 可修改项目

- [x] 人物状态
- [x] 能力熟练
- [x] 特技属性
- [x] 装备道具
- [x] 支援关系
- [x] 金钱奖励

#### 项目文件结构

```mermaid
graph LR;
R{{PoR}}
m([Modifier])
r([Resource])
i([interface])
p([parameter])
s([structure])
w([widget])
R---m
R---PoR.pro
R---README.MD
R---requirements.txt
r---AFF([AFF])
r---IID([IID])
r---PID([PID])
r---QM([QM])
r---WP([WP])
r---blank.png
r---icon.png
r---resource.qrc
m---i
m---p
m---w
m---s
m---PoR.pyw
i---ability.py
i---item.py
i---resource.py
i---skill.py
i---status.py
i---support.py
i---window.py
p---data_setting.py
p---enum_data.py
s---text.py
s---value.py
w---background_frame.py
w---bool_check.py
w---customize.py
w---map_combo.py
w---name_label.py
w---slot_list.py
w---support_bar.py
w---value_spin.py
R---r
```

