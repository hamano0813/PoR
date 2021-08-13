#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QMainWindow, QPushButton, QTabWidget, QHBoxLayout, QVBoxLayout
from PySide6.QtCore import Qt, QSize
from PySide6.QtGui import QIcon, QCloseEvent, QPixmap
from dolphin_memory_engine import un_hook, is_hooked
from widget import SlotList, BackgroundFrame
from parameter import DataSetting
from . import Status, Ability, Skill, Item, Support, Other


class Window(QMainWindow):
    # noinspection PyTypeChecker,PyUnresolvedReferences
    def __init__(self):
        super(Window, self).__init__(parent=None, flags=Qt.WindowCloseButtonHint)
        self.slot_list = SlotList(None, PID=DataSetting()['人物'], JID=DataSetting()['职业'])
        self.slot_list.setIconSize(QSize(50, 50))
        self.slot_list.setFixedWidth(300)
        refresh_button = QPushButton('刷新列表')
        refresh_button.setFixedHeight(40)
        slot_layout = QVBoxLayout()
        slot_layout.addWidget(self.slot_list)
        slot_layout.addWidget(refresh_button)

        refresh_button.clicked.connect(self.refresh)

        status_frame = Status(self.slot_list)
        ability_frame = Ability(self.slot_list)
        skill_frame = Skill(self.slot_list)
        item_frame = Item(self.slot_list)
        support_frame = Support(self.slot_list)
        other_frame = Other(None)

        self.slot_list.add_child(status_frame)
        self.slot_list.add_child(ability_frame)
        self.slot_list.add_child(skill_frame)
        self.slot_list.add_child(item_frame)
        self.slot_list.add_child(support_frame)
        self.slot_list.add_child(other_frame)

        self.tab_widget = QTabWidget()
        self.tab_widget.addTab(status_frame, '状态')
        self.tab_widget.addTab(ability_frame, '能力')
        self.tab_widget.addTab(skill_frame, '技能')
        self.tab_widget.addTab(item_frame, '装备')
        self.tab_widget.addTab(support_frame, '支援')
        self.tab_widget.addTab(other_frame, '其他')

        main_frame = BackgroundFrame()
        main_layout = QHBoxLayout()
        main_layout.addLayout(slot_layout)
        main_layout.addWidget(self.tab_widget)
        main_frame.setLayout(main_layout)

        self.setCentralWidget(main_frame)
        self.setWindowTitle('苍炎的轨迹 动态修改器 V1.2')
        self.setWindowIcon(QIcon(':/ICON/icon.png'))
        self.setMinimumHeight(480)
        self.refresh()

        skill_frame['SID_EQUIPLIGHT'].stateChanged.connect(self.charge_light)

    def refresh(self):
        self.slot_list.refresh()
        self.tab_widget.setEnabled(bool(self.slot_list.SLOT_MAPPING()))

    def charge_light(self):
        label = self.centralWidget().layout().itemAt(1).widget().widget(1).layout().itemAtPosition(8, 3).widget()
        light = 'LIGHT' if self.sender().isChecked() else 'STAFF'
        label.setPixmap(QPixmap(f':WP/WP_{light}.png'))

    def closeEvent(self, event: QCloseEvent) -> None:
        if is_hooked():
            un_hook()
        event.accept()
