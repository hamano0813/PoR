#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGridLayout, QSpacerItem, QSizePolicy
from PySide6.QtGui import QPixmap
from widget import BackgroundFrame, ValueSpin, SupportBar, NameLabel
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Support(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        self.support_row = list()

        self.current_aff = NameLabel('')
        self.current_aff.setFixedSize(30, 30)
        self.current_name = NameLabel('')
        self.current_name.setFixedSize(100, 30)

        main_layout = QGridLayout()
        main_layout.addWidget(self.current_aff, 0, 0, 1, 1)
        main_layout.addWidget(self.current_name, 0, 1, 1, 1)
        for idx in range(1, 8):
            aff_label = NameLabel('')
            aff_label.setFixedSize(30, 30)
            name_label = NameLabel('')
            name_label.setFixedSize(100, 30)
            self[f'支援{idx}'] = ValueSpin(self, value=DataSetting()[f'支援{idx}'])
            progress_c = SupportBar('Ｃ')
            progress_b = SupportBar('Ｂ')
            progress_a = SupportBar('Ａ')
            self.support_row.append([aff_label, name_label, self[f'支援{idx}'], progress_c, progress_b, progress_a])
            main_layout.addWidget(aff_label, idx, 0, 1, 1)
            main_layout.addWidget(name_label, idx, 1, 1, 1)
            main_layout.addWidget(self[f'支援{idx}'], idx, 2, 1, 1)
            main_layout.addWidget(progress_c, idx, 3, 1, 1)
            main_layout.addWidget(progress_b, idx, 4, 1, 1)
            main_layout.addWidget(progress_a, idx, 5, 1, 1)

        for row in self.support_row:
            row[2].valueChanged[int].connect(row[3].set_value)
            row[2].valueChanged[int].connect(row[4].set_value)
            row[2].valueChanged[int].connect(row[5].set_value)

        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 9, 6, 1, 1)
        main_layout.setSpacing(3)
        self.setLayout(main_layout)

    def set_support(self, name: str):
        current = EnumData().PID_ENUM.get(name)
        self.current_name.setText(current)
        self.current_aff.setPixmap(QPixmap(f':AFF/{EnumData().AFF_ENUM.get(current, "blank")}.gif'))
        if support_data := EnumData().SUPPORT_MAPPING().get(name):
            for idx, (pid, data) in enumerate(support_data):
                if pid:
                    self.support_row[idx][1].setText(pid)
                    self.support_row[idx][0].setPixmap(QPixmap(f':AFF/{EnumData().AFF_ENUM[pid]}.gif'))
                    self.support_row[idx][3].setRange(0, data[0] + 1)
                    self.support_row[idx][4].setRange(data[0] + 1, data[1] + 2)
                    self.support_row[idx][5].setRange(data[1] + 2, data[2] + 3)
                    self.support_row[idx][2].refresh()
                    self.support_row[idx][3].set_value(self.support_row[idx][2].value())
                    self.support_row[idx][4].set_value(self.support_row[idx][2].value())
                    self.support_row[idx][5].set_value(self.support_row[idx][2].value())
                else:
                    self.support_row[idx][3].setRange(0, 0)
                    self.support_row[idx][4].setRange(0, 0)
                    self.support_row[idx][5].setRange(0, 0)
                for widget in self.support_row[idx]:
                    widget.setHidden(not bool(pid))
        else:
            for row in self.support_row:
                for widget in row:
                    widget.setHidden(True)

    def refresh(self):

        name = self._parent.current_pid()
        self.set_support(name)
