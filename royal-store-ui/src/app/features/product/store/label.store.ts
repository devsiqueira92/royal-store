import {
  createLabel,
  deleteLabel,
  loadConfiguration,
  loadConfigurations,
  loadLabel,
  loadPrintOptions,
  loadLabels,
  setLabelActive,
  updateLabel,
} from './label.effects';

export const labelStore = {
  loadLabels,
  loadConfigurations,
  loadConfiguration,
  loadPrintOptions,
  loadLabel,
  createLabel,
  updateLabel,
  setLabelActive,
  deleteLabel,
};
