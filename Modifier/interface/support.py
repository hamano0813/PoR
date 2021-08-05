#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGridLayout, QSpacerItem, QSizePolicy
from widget import BackgroundFrame, ValueSpin, SupportBar, NameLabel
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Support(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        self.support_row = list()

        main_layout = QGridLayout()
        for idx in range(1, 8):
            label = NameLabel('')
            label.setFixedSize(100, 28)
            self[f'支援{idx}'] = ValueSpin(self, value=DataSetting()[f'支援{idx}'])
            progress_c = SupportBar('C')
            progress_b = SupportBar('B')
            progress_a = SupportBar('A')
            self.support_row.append([label, self[f'支援{idx}'], progress_c, progress_b, progress_a])
            main_layout.addWidget(label, idx - 1, 0, 1, 1)
            main_layout.addWidget(self[f'支援{idx}'], idx - 1, 1, 1, 1)
            main_layout.addWidget(progress_c, idx - 1, 2, 1, 1)
            main_layout.addWidget(progress_b, idx - 1, 3, 1, 1)
            main_layout.addWidget(progress_a, idx - 1, 4, 1, 1)
        
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 8, 5, 1, 1)
        main_layout.setSpacing(3)
        self.setLayout(main_layout)

    def set_support(self, name: str):
        if support_data := EnumData().SUPPORT_MAPPING().get(name):
            for idx, (pid, data) in enumerate(support_data):
                if pid:
                    self.support_row[idx][0].setText(pid)
                    self.support_row[idx][2].setRange(0, data[0] + 1)
                    self.support_row[idx][3].setRange(data[0] + 1, data[1] + 2)
                    self.support_row[idx][4].setRange(data[1] + 2, data[2] + 3)
                    self.support_row[idx][1].refresh()
                    self.support_row[idx][2].refresh()
                    self.support_row[idx][3].refresh()
                    self.support_row[idx][4].refresh()
                else:
                    self.support_row[idx][2].setRange(0, 0)
                    self.support_row[idx][3].setRange(0, 0)
                    self.support_row[idx][4].setRange(0, 0)
                for widget in self.support_row[idx]:
                    widget.setHidden(not bool(pid))
        else:
            for row in self.support_row:
                for widget in row:
                    widget.setHidden(True)

    def refresh(self):
        for row in self.support_row:
            row[1].valueChanged[int].connect(row[2].set_value)
            row[1].valueChanged[int].connect(row[3].set_value)
            row[1].valueChanged[int].connect(row[4].set_value)
        name = self._parent.current_pid()
        self.set_support(name)
        for child in self._child:
            child.refresh()
