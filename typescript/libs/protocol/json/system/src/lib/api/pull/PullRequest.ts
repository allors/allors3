import { Procedure } from "../../data/Procedure";
import { Pull } from "../../data/Pull";

export interface PullRequest {
  /** List of Pulls */
  l?: Pull[];

  /** Procedure */
  p?: Procedure;
}
