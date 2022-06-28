import { ISelectable } from '@shared/components/ISelectable';
import { asiaGuid } from '../entity.model';
import { asiaPerson } from '../person.Model';
import { asiaInfluencesRelation } from '../relations/influencesRelation.model';

export type SelectableViewBox = { x: number; y: number; width: number; height: number; rotationRad: number };

export type GraphEditorSelectableData = {
  selectable: ISelectable;
  id: asiaGuid;
  data: asiaPerson | asiaInfluencesRelation;
  viewBox: SelectableViewBox;
};
