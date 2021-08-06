#!/usr/bin/env python
# -*- coding: utf-8 -*-


class DataSetting:
    STEP = 0x280
    COUNT = 0x9F
    SLOT = 0x802AF5E4

    def __init__(self):
        self.setting = {
            '模型': (0x802AF760, 0x4),
            'TEMP1': (0x802AF764, 0x4),
            'TEMP2': (0x802AF768, 0x4),
            '人物': (0x802AF76C, 0x4),
            '职业': (0x802AF770, 0x4),
            '阵营': (0x802AF774, 0x4),
            '同行': (0x802AF778, 0x4),
            '序号': (0x802AF77C, 0x1),
            '部队': (0x802AF77D, 0x1),
            '等级': (0x802AF77E, 0x1),
            '经验': (0x802AF77F, 0x1),
            '化身': (0x802AF780, 0x1),
            'TEMP3': (0x802AF781, 0x1),
            '坐标Ｘ': (0x802AF782, 0x1),
            '坐标Ｙ': (0x802AF783, 0x1),
            'TEMP4': (0x802AF784, 0x2),
            '首领': (0x802AF786, 0x1, 0),
            'TEMP5': (0x802AF786, 0x1, (1, 7)),
            '行动': (0x802AF787, 0x1, 0),
            '救出': (0x802AF787, 0x1, 1),
            '被救': (0x802AF787, 0x1, 2),
            'TEMP6': (0x802AF787, 0x1, (3, 7)),
            '麻痹': (0x802AF788, 0x2, (0, 3)),
            'TEMP7': (0x802AF788, 0x2, 3),
            '沉默': (0x802AF788, 0x2, (4, 7)),
            '睡眠': (0x802AF788, 0x2, (7, 10)),
            '狂暴': (0x802AF788, 0x2, (10, 13)),
            '中毒': (0x802AF788, 0x2, (13, 16)),
            'TEMP8': (0x802AF789, 0x3),
            'ＨＰ': (0x802AF78C, 0x1),
            'S_CON': (0x802AF78D, 0x1),
            'S_MOV': (0x802AF78E, 0x1),
            'S_HP': (0x802AF78F, -0x1),
            'S_STR': (0x802AF790, -0x1),
            'S_MAG': (0x802AF791, -0x1),
            'S_SKL': (0x802AF792, -0x1),
            'S_SPD': (0x802AF793, -0x1),
            'S_LUK': (0x802AF794, -0x1),
            'S_DEF': (0x802AF795, -0x1),
            'S_MDF': (0x802AF796, -0x1),
            'TEMP9': (0x802AF797, 0x1),
            'E_HP': (0x802AF798, 0x1),
            'E_STR': (0x802AF799, 0x1),
            'E_MAG': (0x802AF79A, 0x1),
            'E_SKL': (0x802AF79B, 0x1),
            'E_SPD': (0x802AF79C, 0x1),
            'E_LUK': (0x802AF79D, 0x1),
            'E_DEF': (0x802AF79E, 0x1),
            'E_MDF': (0x802AF79F, 0x1),

            '道具1': (0x802AF7B0, 0x4),
            '道具2': (0x802AF7B8, 0x4),
            '道具3': (0x802AF7C0, 0x4),
            '道具4': (0x802AF7C8, 0x4),
            '道具5': (0x802AF7D0, 0x4),
            '道具6': (0x802AF7D8, 0x4),
            '道具7': (0x802AF7E0, 0x4),
            '道具8': (0x802AF7E8, 0x4),
            '耐久1': (0x802AF7B4, 0x1),
            '耐久2': (0x802AF7BC, 0x1),
            '耐久3': (0x802AF7C4, 0x1),
            '耐久4': (0x802AF7CC, 0x1),
            '耐久5': (0x802AF7D4, 0x1),
            '耐久6': (0x802AF7DC, 0x1),
            '耐久7': (0x802AF7E4, 0x1),
            '耐久8': (0x802AF7EC, 0x1),
            '装备1': (0x802AF7B5, 0x1, 7),
            '装备2': (0x802AF7BD, 0x1, 7),
            '装备3': (0x802AF7C5, 0x1, 7),
            '装备4': (0x802AF7CD, 0x1, 7),
            '装备5': (0x802AF7D5, 0x1, 7),
            '装备6': (0x802AF7DD, 0x1, 7),
            '装备7': (0x802AF7E5, 0x1, 7),
            '装备8': (0x802AF7ED, 0x1, 7),
            '掉落1': (0x802AF7B5, 0x1, 6),
            '掉落2': (0x802AF7BD, 0x1, 6),
            '掉落3': (0x802AF7C5, 0x1, 6),
            '掉落4': (0x802AF7CD, 0x1, 6),
            '掉落5': (0x802AF7D5, 0x1, 6),
            '掉落6': (0x802AF7DD, 0x1, 6),
            '掉落7': (0x802AF7E5, 0x1, 6),
            '掉落8': (0x802AF7ED, 0x1, 6),

            '剑': (0x802AF7F0, 0x1),
            '枪': (0x802AF7F1, 0x1),
            '斧': (0x802AF7F2, 0x1),
            '弓': (0x802AF7F3, 0x1),
            '炎': (0x802AF7F4, 0x1),
            '雷': (0x802AF7F5, 0x1),
            '风': (0x802AF7F6, 0x1),
            '杖': (0x802AF7F7, 0x1),

            '支援1': (0x802AF7F8, 0x1),
            '支援2': (0x802AF7F9, 0x1),
            '支援3': (0x802AF7FA, 0x1),
            '支援4': (0x802AF7FB, 0x1),
            '支援5': (0x802AF7FC, 0x1),
            '支援6': (0x802AF7FD, 0x1),
            '支援7': (0x802AF7FE, 0x1),

            '战斗': (0x802AF80C, 0x2),
            '胜利': (0x802AF80E, 0x2),

            'SID_HERO': (0x802AF7A0, 0x4, 0),
            'SID_FEMALE': (0x802AF7A0, 0x4, 1),
            'SID_HIGHER': (0x802AF7A0, 0x4, 2),
            'SID_TWICE': (0x802AF7A0, 0x4, 3),
            'SID_FLY': (0x802AF7A0, 0x4, 4),
            'SID_VILLAGEDESTROY': (0x802AF7A0, 0x4, 5),
            'SID_BOSS': (0x802AF7A0, 0x4, 6),
            'SID_FINAL': (0x802AF7A0, 0x4, 7),
            'SID_ANIMALIZE': (0x802AF7A0, 0x4, 8),
            'SID_LYCANTHROPE': (0x802AF7A0, 0x4, 9),
            'SID_FIXEDBEAST': (0x802AF7A0, 0x4, 10),
            'SID_ELITE': (0x802AF7A0, 0x4, 11),
            'SID_TURNREGENE': (0x802AF7A0, 0x4, 12),
            'SID_BARGAIN': (0x802AF7A0, 0x4, 13),
            'SID_SWIFT': (0x802AF7A0, 0x4, 14),
            'SID_WLUPTOS': (0x802AF7A0, 0x4, 15),
            'SID_BLAVE': (0x802AF7A0, 0x4, 16),
            'SID_IMPREGNABLE': (0x802AF7A0, 0x4, 17),
            'SID_TEMPER': (0x802AF7A0, 0x4, 18),
            'SID_CALM': (0x802AF7A0, 0x4, 19),
            'SID_FLYTHRU': (0x802AF7A0, 0x4, 20),
            'SID_WALKTHRU': (0x802AF7A0, 0x4, 21),
            'SID_RESCUEP': (0x802AF7A0, 0x4, 22),
            'SID_HANDI': (0x802AF7A0, 0x4, 23),
            'SID_CHANT': (0x802AF7A0, 0x4, 24),
            'SID_CHANTHP': (0x802AF7A0, 0x4, 25),
            'SID_CHANTSTR': (0x802AF7A0, 0x4, 26),
            'SID_CHANTMPOW': (0x802AF7A0, 0x4, 27),
            'SID_CHANTTECH': (0x802AF7A0, 0x4, 28),
            'SID_CHANTQUICK': (0x802AF7A0, 0x4, 29),
            'SID_CHANTLUCK': (0x802AF7A0, 0x4, 30),
            'SID_CHANTDEF': (0x802AF7A0, 0x4, 31),
            'SID_CHANTMDEF': (0x802AF7A4, 0x4, 0),
            'SID_EQUIPFANG': (0x802AF7A4, 0x4, 1),
            'SID_EQUIPKNIFE': (0x802AF7A4, 0x4, 2),
            'SID_EQUIPLIGHT': (0x802AF7A4, 0x4, 3),
            'SID_EQSW': (0x802AF7A4, 0x4, 4),
            'SID_EQLA': (0x802AF7A4, 0x4, 5),
            'SID_EQAX': (0x802AF7A4, 0x4, 6),
            'SID_EQBW': (0x802AF7A4, 0x4, 7),
            'SID_EQRD': (0x802AF7A4, 0x4, 8),
            'SID_AMBUSH': (0x802AF7A4, 0x4, 9),
            'SID_GRASP': (0x802AF7A4, 0x4, 10),
            'SID_ANGER': (0x802AF7A4, 0x4, 11),
            'SID_DEFENCE': (0x802AF7A4, 0x4, 12),
            'SID_WINGSHIELD': (0x802AF7A4, 0x4, 13),
            'SID_PRAY': (0x802AF7A4, 0x4, 14),
            'SID_SUNTRICK': (0x802AF7A4, 0x4, 15),
            'SID_MOONTRICK': (0x802AF7A4, 0x4, 16),
            'SID_STARTRICK': (0x802AF7A4, 0x4, 17),
            'SID_CONTINUATION': (0x802AF7A4, 0x4, 18),
            'SID_ASSASSINATE': (0x802AF7A4, 0x4, 19),
            'SID_WEAPONDESTROY': (0x802AF7A4, 0x4, 20),
            'SID_SNIPE': (0x802AF7A4, 0x4, 21),
            'SID_CHAOS': (0x802AF7A4, 0x4, 22),
            'SID_COUNTER': (0x802AF7A4, 0x4, 23),
            'SID_RUMBLE': (0x802AF7A4, 0x4, 24),
            'SID_IMPACT': (0x802AF7A4, 0x4, 25),
            'SID_CHARISMA': (0x802AF7A4, 0x4, 26),
            'SID_HORROR': (0x802AF7A4, 0x4, 27),
            'SID_STEAL': (0x802AF7A4, 0x4, 28),
            'SID_PROVOKE': (0x802AF7A4, 0x4, 29),
            'SID_SHADE': (0x802AF7A4, 0x4, 30),
            'SID_GAMBLE': (0x802AF7A4, 0x4, 31),
            'SID_ABSMOVE': (0x802AF7A8, 0x4, 0),
            'SID_FAIRNESS': (0x802AF7A8, 0x4, 1),
            'SID_CONFRONT': (0x802AF7A8, 0x4, 2),
            'SID_FLUTTER': (0x802AF7A8, 0x4, 3),
            'SID_TACKLE': (0x802AF7A8, 0x4, 4),
            'SID_TACKLE2': (0x802AF7A8, 0x4, 5),
            'SID_SNARL': (0x802AF7A8, 0x4, 6),
            'SID_EVILEYE': (0x802AF7A8, 0x4, 7),
            'SID_REINFORCEMENTS': (0x802AF7A8, 0x4, 8),
            'SID_EARTHBLESSING': (0x802AF7A8, 0x4, 9),
            'SID_SKYBLESSING': (0x802AF7A8, 0x4, 10),
            'SID_FRAC90': (0x802AF7A8, 0x4, 11),
            'SID_SUNMOON': (0x802AF7A8, 0x4, 12),
            'SID_EVENTCC': (0x802AF7A8, 0x4, 13),
            'SID_SUMMONED': (0x802AF7A8, 0x4, 14),
            'SID_SHOOT': (0x802AF7A8, 0x4, 15),
            'SID_KEY50': (0x802AF7A8, 0x4, 16),
            'SID_TELEGNOSIS': (0x802AF7A8, 0x4, 17),
            'SID_BIGEAR': (0x802AF7A8, 0x4, 18),
            'SID_GODDESSBLESS': (0x802AF7A8, 0x4, 19),
            'SID_TRI_A': (0x802AF7A8, 0x4, 20),
            'SID_TRI_B': (0x802AF7A8, 0x4, 21),
            'SID_EQ_A': (0x802AF7A8, 0x4, 22),
            'SID_EQ_B': (0x802AF7A8, 0x4, 23),
            'SID_EQ_C': (0x802AF7A8, 0x4, 24),
            'SID_TEMP_ON_DIE': (0x802AF7A8, 0x4, 25),
            'SID_BRIGHTNESS': (0x802AF7A8, 0x4, 26),
            'SID_KEY0': (0x802AF7A8, 0x4, 27),
            'SID_IMMORTAL': (0x802AF7A8, 0x4, 28),
            'SID_EQ_D': (0x802AF7A8, 0x4, 29),
            'SID_EQREV_A': (0x802AF7A8, 0x4, 30),
            'SID_WEAK_A': (0x802AF7A8, 0x4, 31),
            'SID_AHIMSA': (0x802AF7AC, 0x4, 0),
        }

    def __getitem__(self, name: str):
        return self.setting.get(name)
