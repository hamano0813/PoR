#!/usr/bin/env python
# -*- coding: utf-8 -*-

from typing import Any, List
from PySide6.QtWidgets import QListView
from PySide6.QtCore import Qt, QAbstractListModel, QModelIndex
from PySide6.QtGui import QIcon
from parameter import DataSetting, EnumData
from .customize import Customize


class SlotList(QListView, Customize):
    def __init__(self, parent, **kwargs):
        QListView.__init__(self, parent)
        Customize.__init__(self, parent, **kwargs)
        self.__child: List[Customize] = list()
        self.pid_mapping = EnumData().PID_MAPPING().copy()
        self.jid_mapping = EnumData().JID_MAPPING().copy()
        self.pid_sequence = list()
        self.jid_sequence = list()
        self.slot = dict()

    # noinspection PyPep8Naming
    def SLOT_MAPPING(self):
        return self.slot

    # noinspection PyPep8Naming
    def CURRENT_PID(self):
        return self.model().data(self.model().createIndex(self.currentIndex(), 0), Qt.ToolTipRole)

    def refresh(self):
        self.disconnect(self)
        self.pid_sequence = self.sequence('PID').copy()
        self.jid_sequence = self.sequence('JID').copy()
        self.setModel(SlotModel(self))
        # noinspection PyUnresolvedReferences
        self.clicked.connect(self.control_child)
        self.slot = dict()
        for idx in range(DataSetting.COUNT):
            if self.model().data(self.model().createIndex(idx, 0), Qt.ToolTipRole) == '00000000':
                self.setRowHidden(idx, True)
            else:
                self.setRowHidden(idx, False)
                self.slot[DataSetting.SLOT + DataSetting.STEP * idx] = self.pid_mapping.get(self.pid_sequence[idx])[-1]
        for widget in self.__child:
            widget.refresh()

    def add_child(self, widget: Customize):
        self.__child.append(widget)

    def control_child(self):
        idx = self.currentIndex().row()
        for widget in self.__child:
            widget.offset = DataSetting.STEP * idx
            widget.refresh()


class SlotModel(QAbstractListModel):
    def __init__(self, parent: SlotList):
        QAbstractListModel.__init__(self, parent)

    def columnCount(self, parent: QModelIndex = ...) -> int:
        return 1

    def rowCount(self, parent: QModelIndex = ...) -> int:
        return DataSetting.COUNT

    def data(self, index: QModelIndex, role: int = ...) -> Any:
        if not index.isValid():
            return None
        pid_data = self.parent().pid_sequence[index.row()]
        jid_data = self.parent().jid_sequence[index.row()]
        pid_enum = self.parent().pid_mapping.get(pid_data, [f'{pid_data:08X}'] * 2)
        jid_enum = self.parent().jid_mapping.get(jid_data, [f'{pid_data:08X}'] * 2)
        if role == Qt.DisplayRole:
            return f'「{pid_enum[-1]}」\n{jid_enum[-1]}'
        if role == Qt.DecorationRole:
            return QIcon(f":/PID/{pid_enum[0]}.gif")
        if role == Qt.ToolTipRole:
            return pid_enum[0]
        return None
