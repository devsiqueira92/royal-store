import {
  createLabel,
  deleteLabel,
  loadLabel,
  loadLabels,
  setLabelActive,
  updateLabel,
} from './label.effects';

export const labelStore = {
  loadLabels,
  loadLabel,
  createLabel,
  updateLabel,
  setLabelActive,
  deleteLabel,
};
