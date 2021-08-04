#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QMainWindow, QPushButton, QTabWidget, QHBoxLayout, QVBoxLayout
from PySide6.QtCore import Qt, QSize
from PySide6.QtGui import QIcon, QCloseEvent
from dolphin_memory_engine import un_hook, is_hooked
from widget import SlotList, BackgroundFrame
from parameter import DataSetting
from . import Status, Item, Support


class Window(QMainWindow):
    # noinspection PyTypeChecker,PyUnresolvedReferences
    def __init__(self):
        super(Window, self).__init__(parent=None, flags=Qt.WindowCloseButtonHint)
        slot_list = SlotList(None, PID=DataSetting()['人物'], JID=DataSetting()['职业'])
        slot_list.setIconSize(QSize(50, 50))
        slot_list.setFixedSize(255, 500)
        refresh_button = QPushButton('刷新列表')
        refresh_button.setFixedHeight(40)
        slot_layout = QVBoxLayout()
        slot_layout.addWidget(slot_list)
        slot_layout.addWidget(refresh_button)
        slot_layout.addStretch()

        refresh_button.clicked.connect(slot_list.refresh)

        status_frame = Status(slot_list)
        item_frame = Item(slot_list)
        support_frame = Support(slot_list)

        slot_list.add_child(status_frame)
        slot_list.add_child(item_frame)
        slot_list.add_child(support_frame)

        tab_widget = QTabWidget()
        tab_widget.addTab(status_frame, '状态')
        tab_widget.addTab(item_frame, '道具')
        tab_widget.addTab(support_frame, '支援')

        main_frame = BackgroundFrame()
        main_layout = QHBoxLayout()
        main_layout.addLayout(slot_layout)
        main_layout.addWidget(tab_widget)
        main_frame.setLayout(main_layout)

        self.setCentralWidget(main_frame)
        self.setWindowTitle('苍炎的轨迹 动态修改器 V1.0')
        self.setWindowIcon(QIcon(':/ICON/icon.png'))
        slot_list.refresh()

    def closeEvent(self, event: QCloseEvent) -> None:
        if is_hooked():
            un_hook()
        event.accept()
